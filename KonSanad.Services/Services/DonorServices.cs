using KonSanad.Core.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace KonSanad.Services.Services
{
    public class DonorServices : IDonorServices
    {
       // private readonly IDonorServices _orderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public DonorServices(
         
            IUnitOfWork unitOfWork,
            IMapper mapper, UserManager<AppUser> userManager)
        {
         
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;

        }

        // Helper to map DonationOrder.Status string to DonationRequestStatus enum
        private DonationRequestStatus MapDonationOrderStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return DonationRequestStatus.Pending;
            var s = status.Trim().ToLower();
            if (s.Contains("قيد") || s.Contains("مراجعة") || s.Contains("pending")) return DonationRequestStatus.Pending;
            if (s.Contains("مكتمل") || s.Contains("completed") || s.Contains("complete")) return DonationRequestStatus.Completed;
            if (s.Contains("مرفوض") || s.Contains("rejected")) return DonationRequestStatus.Rejected;
            return DonationRequestStatus.Pending;
        }


        // ---------------------------------------------------------------------------
        // 2) Get All Low-Stock Supplies
        // ---------------------------------------------------------------------------

        //  new : pagenation
        public async Task<List<SupplyDto>> GetLowStockSuppliesAsync(int pageNumber, int pageSize)
        {
            var supplies = await _unitOfWork.Repository<Supply, int>()
                .GetQueryable()
                .Where(s => s.StockQuantity <= s.MinimumLevel)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return supplies.Select(s => new SupplyDto
            {
                Id = s.Id,
                Name = s.Name,
                Quantity = s.StockQuantity ?? 0,
                imageurl = s.ImageUrl,
                Unit = s.Unit,
                title = s.Title
            }).ToList();
        }

        public async Task<IReadOnlyList<CashDonationDto>> GetMyCashDonationsAsyncc(string userId)
        {
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);
            if (donor is null)
                throw new Exception($"Donor not found for userId: {userId}");

            // جيب كل الـ CashDonations
            var cashDonations = await _unitOfWork.Repository<CashDonation, int>()
       .FindAllAsync(
           cd => cd.HelpOrderId != 0,  // التعديل ال عملته علشان ارجع التبرعات بالكاش لل HelpOrderId بس 
           include: q => q
               .Include(cd => cd.DonationOrder)
               .Include(cd => cd.Receipt)
       );
           
            // فلتر في الميموري زي الـ SQL
            var result = cashDonations
                .Where(cd => cd.DonationOrder != null && cd.DonationOrder.DonorId == donor.Id)
                .Select(d => new CashDonationDto
                {
                    Id = d.Id,

                    SupplyName = _unitOfWork.Repository<Supply, int>()
            .FirstOrDefaultAsync(s => s.Id == d.HelpOrderId).Result?.Name,
                    CreatedAt = d.CreatedAt,
                    Status = d.Status,
                    CashAmount = d.Amount,
                    
                    //new status
                    cashDonationStatus = d.cashDonationStatus,
                    
                    ReceiptIds = d.Receipt != null
                        ? new List<int> { d.Receipt.Id }
                        : new List<int>()
                }).ToList();

            return result;
        }
        //-----------------------------------------------------------------------------------
        public async Task<IEnumerable<ItemDonationDto>> GetMyItemDonationsAsync(string userId)
        {
            // 1) Get donor by AppUserId
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (donor == null)
                throw new Exception("Donor not found");

            // 2) Query DonationOrders that have Items only
            var itemDonations = await _unitOfWork.Repository<DonationOrder, int>()
                .FindAllAsync(
                    d => d.DonorId == donor.Id && d.OrderItems.Any(),
                    include: q => q
                        .Include(d => d.OrderItems)
                            .ThenInclude(oi => oi.Supply)
                        .Include(d => d.Receipts)
                );

            // 3) Map to DTO
            var result = itemDonations.Select(d => new ItemDonationDto
            {
                Id = d.Id,
                OrderDate = d.OrderDate,
                CreatedAt = d.CreatedAt,
                Status = d.Status,
                Items = d.OrderItems?.Select(oi => new DonationItemDetailDto
                {
                    SupplyId = oi.SupplyId ?? 0,
                    Quantity = oi.Quantity,
                    CustomName = oi.CustomSupplyName,
                    ImageFile = oi.ImageUrl,
                    Unit = oi.Unit
                }).ToList() ?? new List<DonationItemDetailDto>(),
                ReceiptIds = d.Receipts?.Select(r => r.Id).ToList() ?? new List<int>()
            }).ToList();

            return result;
        }

        // Get all cash donations made by the donor, including related receipts
       

        #region GetMyItemDonationsAsync
        //-----------------------------------------------------------------------------------

        public async Task<IEnumerable<MyOrderItemDto>> GetMyDonatedOrderItemsAsync(string userId)
        {
            // 1️⃣ هات المتبرع
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            //لو ملقاش المتبرع ال فالتوكين بيساوي ال فجدول ال appUserId
            if (donor == null)
                throw new Exception("Donor not found");


            // 2️⃣ هات OrderItems باستخدام Repository
            // جلب الـ OrderItems الخاصة بالمتبرع

            var orderItems = await _unitOfWork.Repository<OrderItem, int>()
                .FindAllAsync(
                    oi => oi.DonationOrder != null &&
                          oi.DonationOrder.DonorId == donor.Id,
                    include: q => q.Include(oi => oi.DonationOrder)
                );

            // 3️⃣ Mapping آمن يمنع Null crash
            // ?? string.Empty دي علشان لو القيمه ب null في الداتا بيز الاسواجر بيضرب

            var result = orderItems.Select(oi => new MyOrderItemDto
            {
                Id = oi.Id,
                DonationOrderId = oi.DonationOrderId,
                SupplyId = oi.SupplyId,
                CustomSupplyName = oi.CustomSupplyName ?? string.Empty,
                Quantity = oi.Quantity,
                Category = oi.Category ?? string.Empty,
                Unit = oi.Unit ?? string.Empty,
                CreatedAt = oi.CreatedAt,
                Status = oi.Status ?? string.Empty,
                ImageUrl = oi.ImageUrl ?? string.Empty,
                Description = oi.Description ?? string.Empty,
                UpdatedAt = oi.UpdatedAt,
                IsDeleted = oi.IsDeleted,

                orderItemStatus = oi.orderItemStatus

            }).ToList();

            return result;
        }
        //-------------------------------------------------------------------------------
        #endregion
        // Get all campaigns that the donor has donated From it .
        // -----------------------------------------------------------
        public async Task<IEnumerable<CampaignDto>> GetMyDonationsInCampaignRequestsAsync(string userId)
        {
            // 1. Get donor by AppUserId
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            // 2. check if donor exists
            if (donor is null)
                throw new Exception($"Donor not found for userId: {userId}");

            var cashDonations = await _unitOfWork.Repository<CashDonation, int>()
       .FindAllAsync(
           cd => cd.CampaignId != null &&
                 cd.CampaignId != 0 &&
                 cd.DonationOrder.DonorId == donor.Id,
           include: q => q
               .Include(cd => cd.DonationOrder)
               .Include(cd => cd.Campaign)
                   .ThenInclude(c => c.Category)
   );

            var campaigns = cashDonations
                //.Where(cd => cd.DonationOrder != null && cd.DonationOrder.DonorId == donor.Id)
                .Select(cd => cd.Campaign)
                .Where(c => c != null)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();

            // 4. Map to CampaignDto and compute a few helpful aggregates
            var result = campaigns.Select(c =>
            {
                var totalCash = cashDonations
       .Where(cd => cd.CampaignId == c.Id)
       .Sum(cd => cd.Amount);
                var donorCount = cashDonations
         .Where(cd => cd.CampaignId == c.Id && cd.DonationOrder?.DonorId != null)
         .Select(cd => cd.DonationOrder.DonorId)
         .Distinct()
         .Count();

                int progressPercentage = 0;
                if (c.GoalCash.HasValue && c.GoalCash > 0)
                {
                    progressPercentage = (int)Math.Min(100,
                        Math.Round((double)(totalCash / c.GoalCash.Value) * 100));
                }
                var myCashInCampaign = cashDonations
    .Where(cd => cd.CampaignId == c.Id)
    .Sum(cd => cd.Amount);
                return new CampaignDto
                {
                    Id = c.Id,

                    Title = c.Title ?? string.Empty,

                    ShortDescription = string.IsNullOrEmpty(c.Description)
                        ? string.Empty
                        : (c.Description.Length > 150 ? c.Description.Substring(0, 150) : c.Description),

                    PictureUrl = c.PictureUrl ?? string.Empty,

                    CategoryName = c.Category?.Name ?? string.Empty,

                    DonationRequestStatus =(int)RequestedStatus.Pending,

                    ProgressPercentage = progressPercentage,
                    CreateAt = c.CreatedAt,
                    CashAmount =myCashInCampaign,
                    DonorCount = donorCount,
                };
            }).ToList();

            // 5. Return result
            return result;

        }
        // -------------------------------------------------------------------

        public async Task<(bool Success, string Message)> UpdateItemQuantityAsync(int orderItemId, int newQuantity)
        {
            var item = await _unitOfWork.Repository<OrderItem, int>()
                .FirstOrDefaultAsync(i => i.Id == orderItemId);

            if (item == null)
                return (false, "المنتج غير موجود");

            item.Quantity = newQuantity;

            _unitOfWork.Repository<OrderItem, int>().Update(item);

            await _unitOfWork.Complete();

            return (true, "تم تعديل الكمية بنجاح");
        }
        // -------------------------------------------------------------------

        public async Task<(bool Success, string Message)> DeleteItemAsync(int orderItemId)
        {
            // 1️⃣ هات الـ OrderItem
            var item = await _unitOfWork.Repository<OrderItem, int>()
                .FirstOrDefaultAsync(i => i.Id == orderItemId);

            if (item == null)
                return (false, "المنتج غير موجود");

            // 2️⃣ هات الـ DonationOrder المرتبط
            var donationOrder = await _unitOfWork.Repository<DonationOrder, int>()
                .FirstOrDefaultAsync(o => o.Id == item.DonationOrderId);

            if (donationOrder == null)
                return (false, "الطلب غير موجود");

            // 3️⃣ هات عدد الـ OrderItems في الـ DonationOrder ده
            var orderItemsCount = await _unitOfWork.Repository<OrderItem, int>()
                .FindAllAsync(i => i.DonationOrderId == donationOrder.Id);
            //
            var orderItems = await _unitOfWork.Repository<OrderItem, int>()
    .FindAllAsync(i => i.DonationOrderId == donationOrder.Id);

            var cashDonations = await _unitOfWork.Repository<CashDonation, int>()
                .FindAllAsync(c => c.DonationOrderId == donationOrder.Id);

            var totalCount = orderItems.Count() + cashDonations.Count();

            // 4️⃣ لو الـ OrderItem الوحيد في الـ DonationOrder، نمسح الـ DonationOrder كمان
            if (totalCount == 1)
            {
                _unitOfWork.Repository<OrderItem, int>().Delete(item);
                _unitOfWork.Repository<DonationOrder, int>().Delete(donationOrder);
            }
            else
            {
                // لو فيه أكتر من OrderItem، نمسح بس الـ OrderItem ده
                _unitOfWork.Repository<OrderItem, int>().Delete(item);
            }

            await _unitOfWork.Complete();

            return (true, "تم الحذف بنجاح");
        }

        //-------------------------------------------------------------------

        public async Task<bool> DeleteCashDonationAsync(int cashDonationId)
        {
            var repo = _unitOfWork.Repository<CashDonation, int>();

            var cashDonation = await repo.FirstOrDefaultAsync(
                cd => cd.Id == cashDonationId,
                include: q => q
                    .Include(cd => cd.DonationOrder)
                    .Include(cd => cd.Receipt)
            );

            if (cashDonation == null)
                return false;

            // حذف الكاش
            repo.Delete(cashDonation);

            // حذف الاوردر
            if (cashDonation.DonationOrder != null)
            {
                _unitOfWork.Repository<DonationOrder, int>()
                    .Delete(cashDonation.DonationOrder);
            }

            await _unitOfWork.Complete();

            return true;
        }
        public async Task<bool> UpdateCashDonationAmountAsync(int cashDonationId, decimal amount)
        {
            var repo = _unitOfWork.Repository<CashDonation, int>();

            var cashDonation = await repo.GetByIdAsync(cashDonationId);

            if (cashDonation == null)
                throw new Exception("CashDonation not found");

            cashDonation.Amount = amount;

            repo.Update(cashDonation);

            await _unitOfWork.Complete();

            return true;
        }

        //del
        public async Task<IEnumerable<CampaignDto>> GetCampaignsDonatedByAsync(string userId)
        {
            // 1) Get donor
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (donor == null)
                throw new Exception("Donor not found");
            var allOrders = await _unitOfWork.Repository<DonationOrder, int>()
    .FindAllAsync(d => d.DonorId == donor.Id);
            Console.WriteLine($"Total orders for donor {donor.Id}: {allOrders.Count()}");
            // جيب الـ CashDonations مع include للـ DonationOrder
            var cashDonations = await _unitOfWork.Repository<CashDonation, int>()
                .FindAllAsync(
                    cd => cd.CampaignId != null,
                    include: q => q
                        .Include(cd => cd.DonationOrder)
                        .Include(cd => cd.Campaign)
                            .ThenInclude(c => c.Category)
                );

            // فلتر بعد ما اتجبت البيانات
            var campaigns = cashDonations
                .Where(cd => cd.DonationOrder != null && cd.DonationOrder.DonorId == donor.Id)
                .Select(cd => cd.Campaign)
                .Where(c => c != null)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();
            // 4) Map to DTO
            var result = campaigns.Select(c =>
            {
                var totalCash = c.DonationOrders.Sum(d => d.CashAmount ?? 0m);

                var donorCount = c.DonationOrders
                    .Where(d => d.DonorId.HasValue)
                    .Select(d => d.DonorId)
                    .Distinct()
                    .Count();

                int progressPercentage = 0;

                if (c.GoalCash.HasValue && c.GoalCash > 0)
                {
                    progressPercentage = (int)Math.Min(100,
                        Math.Round((double)(totalCash / c.GoalCash.Value) * 100));
                }

                return new CampaignDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    ShortDescription = string.IsNullOrEmpty(c.Description)
                        ? ""
                        : (c.Description.Length > 150 ? c.Description.Substring(0, 150) : c.Description),

                    PictureUrl = c.PictureUrl,
                    CategoryName = c.Category?.Name ?? "",
                    Status = (c.EndDate.HasValue && c.EndDate.Value < DateTime.UtcNow)
                        ? "Completed"
                        : "Active",

                    ProgressPercentage = progressPercentage,
                    DonorCount = donorCount
                };

            }).ToList();

            return result;
        }
        // ---------------------------------------------------------------------------
        // 4) Get Profile To Donor 
        // ---------------------------------------------------------------------------
        public async Task<GetProfileDto> GetProfileAsync(string userId)
        {
            // 1) استخدم UserManager للحصول على AppUser
            var appUser = await _userManager.FindByIdAsync(userId);

            if (appUser == null)
                throw new Exception("User not found");

            // 2) ثم احصل على Donor
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (donor is null)
                throw new Exception("Donor not found");

            // 3) Map to DTO
            var profileDto = new GetProfileDto
            {

                FullName = donor.FullName ?? string.Empty,
                NationalId = donor.NationalId ?? string.Empty,
                PhoneNumber = appUser.PhoneNumber ?? string.Empty,
                Address = donor.Address ?? string.Empty,
                Email = appUser.Email ?? string.Empty
            };
            return profileDto;
        }


        // ---------------------------------------------------------------------------
        // 5) Update Profile To Donor
        // ---------------------------------------------------------------------------
        public async Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var donorRebo = _unitOfWork.Repository<Donor, int>();

            var donor = await donorRebo.FirstOrDefaultAsync(
                d => d.AppUserId == userId
            );

            //var dtos = dto;

            //foreach (var prop in typeof(UpdateProfileDto).GetProperties())
            //{
            //    var value = prop.GetValue(dtos) as string;
            //    if (value == "string")
            //    {
            //        prop.SetValue(dtos, null);
            //    }
            //}

            if (donor is null)
                throw new Exception("Donor not found");

            // Update fields only when a value is provided (non-null/non-whitespace)
            if (!string.IsNullOrWhiteSpace(dto.FullName) && !(dto.FullName == "string"))
                donor.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.NationalId) && !(dto.NationalId == "string"))
                donor.NationalId = dto.NationalId;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !(dto.PhoneNumber == "string"))
                donor.Phone = dto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(dto.Address) && !(dto.Address == "string"))
                donor.Address = dto.Address;

            // If email is provided, update Identity user email (and optionally beneficiary.Email)
            if (!string.IsNullOrWhiteSpace(dto.Email) && !(dto.Email == "string"))
            {
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser is null)
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
                donor.Email = dto.Email;
            }
            // If PhoneNumber is provided, update Identity user phone number
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !(dto.PhoneNumber == "string"))
            {
                var appUser = await _userManager.FindByIdAsync(userId);
                if (appUser is null)
                    throw new Exception("App user not found");
                if (!string.Equals(appUser.PhoneNumber, dto.PhoneNumber, StringComparison.OrdinalIgnoreCase))
                {
                    var result = await _userManager.SetPhoneNumberAsync(appUser, dto.PhoneNumber);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(';', result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to update phone number: {errors}");
                    }
                }

                // Keep donor.Phone in sync
                donor.Phone = dto.PhoneNumber;
            }


            _unitOfWork.Repository<Donor, int>().Update(donor);
            await _unitOfWork.Complete();

            return true;
        }

      
    }
}

