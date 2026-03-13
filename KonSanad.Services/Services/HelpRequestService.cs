
using KonSanad.Core.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace KonSanad.Services.Services
{
    public class HelpRequestService : IHelpRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;


        public HelpRequestService(IUnitOfWork unitOfWork , IMapper  _mapper, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;

        }

        /*   public async Task<HelpRequestResponseDto> RequestExistingProductAsync(int id, ExistingProductRequestDto request)
           {

               if (request == null) throw new System.ArgumentNullException(nameof(request));

               // find the order item
               var orderItem = await _unitOfWork.Repository<OrderItem, int>().GetByIdAsync(request.OrderItemId);
               if (orderItem == null) return new HelpRequestResponseDto { HelpOrderId = 0, Message = "Order item not found." };

               // verify this order item belongs to a donation (donor) and is not already assigned to a beneficiary
               var donationOrder = await _unitOfWork.Repository<DonationOrder, int>().GetByIdAsync(orderItem.DonationOrderId);
               if (donationOrder == null) return new HelpRequestResponseDto { HelpOrderId = 0, Message = "Order item does not belong to a donation order." };

               if (!donationOrder.DonorId.HasValue)
                   return new HelpRequestResponseDto { HelpOrderId = 0, Message = "This donation has no donor and cannot be requested." };

               // ensure donation is not already assigned to any beneficiary (via Receipts)
               var receipts = await _unitOfWork.Repository<Receipt, int>().GetAllAsync();
               var assigned = receipts.Any(r => r.DonationOrderId == donationOrder.Id && r.BeneficiaryId.HasValue);
               if (assigned)
                   return new HelpRequestResponseDto { HelpOrderId = 0, Message = "This donated order is already assigned to a beneficiary." };

               // optional: prevent requesting items that belong to campaigns (uncomment if desired)
               // if (donationOrder.CampaignId.HasValue)
               //     return new HelpRequestResponseDto { HelpOrderId = 0, Message = "This donation belongs to a campaign and cannot be requested here." };

               // create HelpOrder with reason and reference to order item
               var helpOrder = new HelpOrder
               {
                   BeneficiaryId = id,
                   Description = (string.IsNullOrWhiteSpace(orderItem.CustomSupplyName) ? string.Empty : $" {orderItem.CustomSupplyName}"),
                   Status = "قيد الانتظار ",
                   IsDeleted = false
               };

               await _unitOfWork.Repository<HelpOrder, int>().AddAsync(helpOrder);
               await _unitOfWork.Complete();

               // ensure we have a Supply record to reference in HelpOrderItem
               int supplyId;
               if (orderItem.SupplyId.HasValue)
               {
                   supplyId = orderItem.SupplyId.Value;
               }
               else
               {
                   // create a catalog-style Supply for this donated custom item so HelpOrderItem can reference it
                   var supply = new Supply
                   {
                       Name = string.IsNullOrWhiteSpace(orderItem.CustomSupplyName) ? "Donated Custom Item" : orderItem.CustomSupplyName,
                       Description = $"Donated item from DonationOrder {donationOrder.Id}",
                       Unit = string.IsNullOrWhiteSpace(orderItem.Unit) ? "pcs" : orderItem.Unit,
                       StockQuantity = 0
                   };

                   await _unitOfWork.Repository<Supply, int>().AddAsync(supply);
                   await _unitOfWork.Complete();

                   supplyId = supply.Id;
               }

               // create HelpOrderItem using quantity from the donated order item
               var helpItem = new HelpOrderItem
               {
                   HelpOrderId = helpOrder.Id,
                   SupplyId = supplyId,
                   QuantityRequested = orderItem.Quantity,
                   ImageUrl = null
               };

               await _unitOfWork.Repository<HelpOrderItem, int>().AddAsync(helpItem);
               await _unitOfWork.Complete();

               return new HelpRequestResponseDto
               {
                   HelpOrderId = helpOrder.Id,
                   Message = "Request submitted successfully."
               };
           }
   */
        #region Update & Delete item

        //Delete item from my request
        public async Task<(bool Success, string Message)> DeleteItemAsync(string userId, int helpOrderItemId)
        {
            // 1️⃣ هات المستفيد
            var beneficiary = await _unitOfWork.Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);
            if (beneficiary == null)
                return (false, "المستفيد غير موجود");

            // 2️⃣ هات الـ HelpOrderItem
            var item = await _unitOfWork.Repository<HelpOrderItem, int>()
                .FirstOrDefaultAsync(i => i.Id == helpOrderItemId);
            if (item == null)
                return (false, "العنصر غير موجود");

            // 3️⃣ هات الـ Order المرتبط بالـ item
            var helpOrder = await _unitOfWork.Repository<HelpOrder, int>()
                .FirstOrDefaultAsync(h => h.Id == item.HelpOrderId);
            if (helpOrder == null || helpOrder.BeneficiaryId != beneficiary.Id)
                return (false, "غير مسموح بحذف هذا العنصر");

            // 4️⃣ احسب عدد المنتجات في الـ HelpOrder
            var itemCount = await _unitOfWork.Repository<HelpOrderItem, int>()
                .FindAllAsync(i => i.HelpOrderId == helpOrder.Id);

            if (itemCount.Count() ==1)
            {
                // لو منتج واحد بس → امسح الـ HelpOrder كله
                _unitOfWork.Repository<HelpOrderItem, int>().Delete(item);
                _unitOfWork.Repository<HelpOrder, int>().Delete(helpOrder);
                await _unitOfWork.Complete();
                return (true, "تم حذف الطلب بالكامل لأنه لم يتبق منتجات");
            }

            // 5️⃣ لو في أكتر من منتج → امسح الـ item بس
            _unitOfWork.Repository<HelpOrderItem, int>().Delete(item);
            await _unitOfWork.Complete();
            return (true, "تم حذف العنصر بنجاح");
        }

        //update item in my request (Quantity)

        public async Task<(bool Success, string Message, HelpOrderItem? UpdatedItem)> UpdateHelpOrderQuantityAsync(int helpOrderItemId, int newQuantity)
        {
            var helpOrderItem = await _unitOfWork.Repository<HelpOrderItem, int>()
                .FirstOrDefaultAsync(h => h.Id == helpOrderItemId);

            if (helpOrderItem == null)
                return (false, "HelpOrderItem مش موجود", null);

            var supply = await _unitOfWork.Repository<OrderItem, int>()
                .FirstOrDefaultAsync(s => s.Id == helpOrderItem.SupplyId);

            if (supply == null)
                return (false, "Supply مش موجود", null);

            if (newQuantity > supply.Quantity)
                return (false, $"الكمية المطلوبة ({newQuantity}) أكبر من المتاح ({supply.Quantity})", null);

            helpOrderItem.QuantityRequested = newQuantity;
            _unitOfWork.Repository<HelpOrderItem, int>().Update(helpOrderItem);
            await _unitOfWork.Complete();

            return (true, "تم التحديث بنجاح", helpOrderItem);
        }



        #endregion

     /*   public async Task<HelpRequestResponseDto> RequestNewProductAsync(int id, NewProductRequestDto request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var imageUrl = await SaveImageAsync(request.ImageFile);

            var helpOrder = new HelpOrder
            {
                BeneficiaryId = id,
                Title = request.ProductName,
                Description = request.ProductDescription,
                Status = "Requested",
                IsDeleted = false
            };

            await _unitOfWork.Repository<HelpOrder, int>().AddAsync(helpOrder);
            await _unitOfWork.Complete();

            var supply = new Supply
            {
                Name = string.IsNullOrWhiteSpace(request.ProductName) ? "Custom Requested Product" : request.ProductName,
                Description = string.IsNullOrWhiteSpace(request.Reason) ? "Requested via HelpOrder (new product)" : request.Reason,
                Unit = "pcs",
                Title ="غذائي",
                StockQuantity = 0
            };

            await _unitOfWork.Repository<Supply, int>().AddAsync(supply);
            await _unitOfWork.Complete();

            var helpItem = new HelpOrderItem
            {
                HelpOrderId = helpOrder.Id,
                SupplyId = supply.Id,
                QuantityRequested = 1,
                reason = request.Reason,
                ImageUrl = imageUrl  // ← الصورة اللي اتخزنت
            };

            await _unitOfWork.Repository<HelpOrderItem, int>().AddAsync(helpItem);
            await _unitOfWork.Complete();

            return new HelpRequestResponseDto
            {
                HelpOrderId = helpOrder.Id,
                Message = "New product request submitted successfully."
            };
        }


*/
        public async Task<string> JoinCampaignAsync(int userid, JoinCampaignRequestDto request)
        {


            // تحقق لو المستفيد موجود
            var beneficiary = await _unitOfWork.Repository<Beneficiary, int>().GetByIdAsync(userid);
            if (beneficiary == null)
                return "المستفيد غير موجود";

            // تحقق لو الحملة موجودة
            // var campaign = await _unitOfWork.Repository<Campaign, int>().GetByIdAsync(request.CampaignId);

            var campaign = await _unitOfWork.Repository<Campaign, int>()
    .FirstOrDefaultAsync(
        c => c.Id == request.CampaignId,
        include: q => q.Include(c => c.BeneficiaryCampaigns)
    );
            if (campaign == null)
                return "الحملة غير موجودة";

            /////////////////////////////////////////////////
            if (CampaignMetrics.DetermineCampaignStatus(campaign) == 2)
            {
                return "الحملة غير نشطة حالياً ولا يمكن الانضمام لها";
            }

            // تحقق لو المستفيد قدم طلب قبل كده لنفس الحملة
            var existingRequest = await _unitOfWork.Repository<BeneficiaryCampaign, int>()
                .FirstOrDefaultAsync(bc => bc.BeneficiaryId == userid
                                           && bc.CampaignId == request.CampaignId);

           

            if (existingRequest != null)
                return "تم تقديم طلب لهذه الحملة بالفعل";
            var cairoZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            var cairoNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cairoZone);

            var joinRequest = new BeneficiaryCampaign
            {
                BeneficiaryId = userid,
                CampaignId = request.CampaignId,
                RequestedAt = cairoNow,
                CreatedAt = cairoNow,
                Status = " قيد الانتظار",
               Reason = request.Reason,
               campaignRequestStatus = RequestedStatus.Pending,
            };

            // joinRequest.Status = "Pending";


            await _unitOfWork.Repository<BeneficiaryCampaign, int>().AddAsync(joinRequest);

           

            // احفظ التغييرات
            await _unitOfWork.Complete();

            return "تم إرسال الطلب بنجاح";
        }

        // 👇 القديمة للمطابقة مع Interface القديم
        public async Task<IEnumerable<CampaignDto>> GetJoinedCampaignsAsync(string userId)
        {
            var result = await GetJoinedCampaignsWithStatusAsync(userId);
            return result.Cast<CampaignDto>();
        }

        // 👇 الجديدة مع الـ enum من الداتابيز
        public async Task<IEnumerable<BeneficiaryJoinedCampaignDto>> GetJoinedCampaignsWithStatusAsync(string userId)
        {
            // 1️⃣ جلب المستفيد
            var beneficiary = await _unitOfWork.Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                throw new Exception("Beneficiary not found");

            // 2️⃣ جلب العلاقات بين المستفيد والحملات
            var joinedCampaigns = await _unitOfWork.Repository<BeneficiaryCampaign, int>()
                .FindAllAsync(
                    bc => bc.BeneficiaryId == beneficiary.Id,
                    include: q => q
                        .Include(x => x.Campaign)
                            .ThenInclude(c => c.DonationOrders)
                        .Include(x => x.Campaign)
                            .ThenInclude(c => c.Category)
                );

            // 3️⃣ نتأكد إن الحملات موجودة
            var campaignDtos = joinedCampaigns
                .Where(j => j.Campaign != null)
                .Select(j => new BeneficiaryJoinedCampaignDto
                {
                    Id = j.Campaign.Id,
                    Title = j.Campaign.Title ?? string.Empty,
                    PictureUrl = j.Campaign.PictureUrl ?? string.Empty,

                    ShortDescription = string.IsNullOrWhiteSpace(j.Campaign.Description)
                        ? string.Empty
                        : j.Campaign.Description.Length > 180
                            ? j.Campaign.Description.Substring(0, 180).Trim() + "…"
                            : j.Campaign.Description,

                    CategoryName = j.Campaign.Category?.Name ?? string.Empty,

                    ProgressPercentage = CampaignMetrics.CalculateProgress(j.Campaign),
                    Status = CampaignMetrics.CalculateStatus(j.Campaign),
                    DonorCount = CampaignMetrics.CalculateDonorCount(j.Campaign),
                    CreateAt = j.Campaign.CreatedAt,

                    // 👈 الحالة فعليًا من الجدول
                    campaignRequestStatus = j.campaignRequestStatus
                }).ToList();

            return campaignDtos;
        }

        //------------------------------------------------------------------

        public async Task<IEnumerable<object>> GetMyHelpRequestsAsync(string userId, int pageNumber, int pageSize)
        {
            var beneficiary = await _unitOfWork.Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary is null)
                throw new Exception("Beneficiary not found");

            var skip = (Math.Max(pageNumber, 1) - 1) * Math.Max(pageSize, 1);
            var take = Math.Max(pageSize, 1);

            var query = _unitOfWork
       .Repository<HelpOrder, int>()
       .GetQueryable()
       .AsNoTracking()
       .Where(h => h.BeneficiaryId == beneficiary.Id);

            var result = await query
                .OrderByDescending(h => h.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(h => new
                {
                    HelpOrderId = h.Id,
                    CreatedAt = h.CreatedAt,
                    Items = h.HelpOrderItems.Select(i => new HelpOrderItemDto
                    {
                        Id = i.Id,
                        HelpOrderId = i.HelpOrderId,
                        SupplyId = i.SupplyId,
                        Quantity = i.QuantityRequested,
                        ImageUrl = i.ImageUrl,
                        Status = i.HelpOrderStatus,
                        Title = i.Title,
                        Reason = i.reason

                    }).ToList()
                })
                .ToListAsync();


            return result;
        }



        public async Task<GetProfileDto> GetProfileAsync(string userId)
        {
            // 1) استخدم UserManager للحصول على AppUser
            var appUser = await _userManager.FindByIdAsync(userId);

            if (appUser == null)
                throw new Exception("User not found");

            // 2) ثم احصل على Beneficiary
            var beneficiary = await _unitOfWork.Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                throw new Exception("Beneficiary not found");

            // 3) Map to DTO
            var profileDto = new GetProfileDto
            {
                
                FullName = beneficiary.FullName ?? string.Empty,
                Email = appUser.Email ?? string.Empty,
                PhoneNumber = appUser.PhoneNumber ?? string.Empty,
                Address = beneficiary.Address ?? string.Empty,
                NationalId = beneficiary.NationalId ?? string.Empty
               
            };

            return profileDto;
        }


        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var beneficiaryRepo = _unitOfWork.Repository<Beneficiary, int>();

            var beneficiary = await beneficiaryRepo.FirstOrDefaultAsync(
                b => b.AppUserId == userId
            );

            if (beneficiary == null)
                throw new Exception("Beneficiary not found");

            // Update fields only when a value is provided (non-null/non-whitespace)
            if (!string.IsNullOrWhiteSpace(dto.FullName) && !(dto.FullName == "string"))
                beneficiary.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.NationalId) && !(dto.NationalId == "string"))
                beneficiary.NationalId = dto.NationalId;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !(dto.PhoneNumber == "string"))
                beneficiary.Phone = dto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(dto.Address) && !(dto.Address == "string"))
                beneficiary.Address = dto.Address;

            // If email is provided, update Identity user email (and optionally beneficiary.Email)
            if (!string.IsNullOrWhiteSpace(dto.Email) && !(dto.Email == "string"))
            {
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser == null)
                    throw new Exception("App user not found");

                if (!string.Equals(appUser.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
                {
                    appUser.Email = dto.Email;
                    // Do not change UserName unless your system expects it; uncomment if needed:
                    // appUser.UserName = dto.Email;

                    var result = await _userManager.UpdateAsync(appUser);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(';', result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to update identity user: {errors}");
                    }
                }

                // Keep beneficiary.Email in sync if you store it there too
                beneficiary.Email = dto.Email;
            }

            _unitOfWork.Repository<Beneficiary, int>().Update(beneficiary);
            await _unitOfWork.Complete();

            return true;
        }


        private async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{fileName}";
        }

        
    }
}