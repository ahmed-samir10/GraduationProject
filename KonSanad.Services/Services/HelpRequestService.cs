
using KonSanad.Core.Entity;
using KonSanad.Core.Entity.Enums;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KonSanad.Services.Services
{
    public class HelpRequestService : IHelpRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IFileService _fileService;  


        public HelpRequestService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<AppUser> userManager,
            INotificationService notificationService,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _fileService = fileService;
            _notificationService = notificationService;
        }
        
        #region Update & Delete item

        //Delete item from my request
        public async Task<string > DeleteItemAsync(string userId, int helpOrderItemId)
        {
            var beneficiary = await _unitOfWork.Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return ( "المستفيد غير موجود");

            var item = await _unitOfWork.Repository<HelpOrderItem, int>()
                .FirstOrDefaultAsync(i => i.Id == helpOrderItemId);

            if (item == null)
                return ( "العنصر غير موجود");

            var helpOrder = await _unitOfWork.Repository<HelpOrder, int>()
                .FirstOrDefaultAsync(h => h.Id == item.HelpOrderId);

            if (helpOrder == null || helpOrder.BeneficiaryId != beneficiary.Id)
                return ( "غير مسموح");

            var count = await _unitOfWork.Repository<HelpOrderItem, int>()
                .CountAsync(i => i.HelpOrderId == helpOrder.Id);

            if (count == 1)
            {
                var mission = await _unitOfWork.Repository<Mission, int>()
    .FirstOrDefaultAsync(m => m.HelpOrderId == helpOrder.Id);

                if (mission != null)
                {
                    _unitOfWork.Repository<Mission, int>().Delete(mission);
                }
                _unitOfWork.Repository<HelpOrderItem, int>().Delete(item);
                _unitOfWork.Repository<HelpOrder, int>().Delete(helpOrder);
            }
            else
            {
                _unitOfWork.Repository<HelpOrderItem, int>().Delete(item);
            }

            await _unitOfWork.Complete();

            await _notificationService.NotifyManagersAsync(new NotificationMessageDto
            {
                Type = NotificationType.MissionRequestSubmitted,
                Title = " تم حذف عنصر من الطلب",
                Message = $" تم حذف عنصر من الطلب رقم {helpOrder.Id}.",
                EntityType = nameof(HelpOrderItem),
                EntityId = item.Id
            });

            return ( count == 1
                ? "تم حذف الطلب بالكامل"
                : "تم حذف العنصر بنجاح");
        }
        //update item in my request (Quantity)

        public async Task<string > UpdateHelpOrderQuantityAsync(int helpOrderItemId, int newQuantity)
        {
            // 1️⃣ هات الـ HelpOrderItem
            var helpOrderItem = await _unitOfWork.Repository<HelpOrderItem, int>()
                .FirstOrDefaultAsync(h => h.Id == helpOrderItemId);

            // 2️⃣ تأكد إن الـ HelpOrderItem موجود
            if (helpOrderItem is null)
                return ( "HelpOrderItem مش موجود");

            // 3️⃣ حدث الكمية
            helpOrderItem.QuantityRequested = newQuantity;
            _unitOfWork.Repository<HelpOrderItem, int>().Update(helpOrderItem);

            // 4️⃣ احفظ التغييرات
            await _unitOfWork.Complete();
                await _notificationService.NotifyManagersAsync(new NotificationMessageDto
                {
                    Type = NotificationType.MissionRequestSubmitted,
                    Title = " تم تحديث عنصر من الطلب",
                    Message = $" تم تحديث عنصر من الطلب رقم {helpOrderItem.HelpOrderId}.",
                    EntityType = nameof(HelpOrderItem),
                    EntityId = helpOrderItem.Id
                });

            // 5️⃣ رجع النتيجة
            return ("تم التحديث بنجاح");
        }



        #endregion

        public async Task<string> JoinCampaignAsync(int userid, JoinCampaignRequestDto request)
        {
            // تحقق لو المستفيد موجود
            var beneficiary = await _unitOfWork.Repository<Beneficiary, int>().GetByIdAsync(userid);
            if (beneficiary == null)
                return "المستفيد غير موجود";

            var campaign = await _unitOfWork.Repository<Campaign, int>()
                .FirstOrDefaultAsync(
                    c => c.Id == request.CampaignId,
                    include: q => q.Include(c => c.BeneficiaryCampaigns)
                );

            if (campaign == null)
                return "الحملة غير موجودة";

            if (CampaignMetrics.DetermineCampaignStatus(campaign) == 2)
                return "الحملة غير نشطة حالياً ولا يمكن الانضمام لها";

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
                Reason = request.Reason,
                campaignRequestStatus = RequestedStatus.Pending,
            };

            await _unitOfWork.Repository<BeneficiaryCampaign, int>().AddAsync(joinRequest);
            await _unitOfWork.Complete();

            await _notificationService.NotifyManagersAsync(new NotificationMessageDto
            {
                Type = NotificationType.BeneficiaryCampaignJoinRequested,
                Title = "طلب انضمام مستفيد",
                Message = $"المستفيد {beneficiary.FullName} طلب الانضمام إلى الحملة {campaign.Title}.",
                EntityType = nameof(BeneficiaryCampaign),
                EntityId = joinRequest.Id
            });

            // CampaignResult - BeneficiaryCount
            if (request.CampaignId > 0)
            {
                var campaignResult = await _unitOfWork.Repository<CampaignResult, int>()
                    .FirstOrDefaultAsync(cr => cr.CampaignId == request.CampaignId);

                if (campaignResult == null)
                {
                    campaignResult = new CampaignResult
                    {
                        CampaignId = request.CampaignId,
                        TotalCashCollected = 0,
                        DonorCount = 0,
                        BeneficiaryCount = 1,
                        VolunteerCount = 0,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        Status = RequestedStatus.Pending,
                        IsDeleted = false
                    };
                    await _unitOfWork.Repository<CampaignResult, int>().AddAsync(campaignResult);
                }
                else
                {
                    campaignResult.BeneficiaryCount += 1;
                    campaignResult.UpdatedAt = DateTime.Now;
                    _unitOfWork.Repository<CampaignResult, int>().Update(campaignResult);
                }

                await _unitOfWork.Complete();
            }

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
        public async Task<string> DeleteHelpRequestAsync(int itemid, string userId)
        {
            var beneficiary = await _unitOfWork.Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return "المستفيد غير موجود";

            var item = await _unitOfWork.Repository<HelpOrderItem, int>()
      .GetQueryable()
      .Include(x => x.HelpOrder)
          .ThenInclude(h => h.HelpOrderItems)
      .FirstOrDefaultAsync(x =>
          x.Id == itemid &&
          x.HelpOrder.BeneficiaryId == beneficiary.Id);

            if (item == null)
                return "المنتج غير موجود";

            var helpOrder = item.HelpOrder;
            var helpOrderId = helpOrder.Id;

            // لو ده آخر منتج في الطلب
            if (helpOrder.HelpOrderItems.Count == 1)
            {
                _unitOfWork.Repository<HelpOrder, int>().Delete(helpOrder);

                await _unitOfWork.Complete();

                await _notificationService.NotifyManagersAsync(
                    new NotificationMessageDto
                    {
                        Title = "حذف طلب مساعدة",
                        Message = $"تم حذف طلب المساعدة رقم {helpOrderId}",
                        EntityType = nameof(HelpOrder),
                        EntityId = helpOrderId
                    });

                return "تم حذف الطلب بالكامل";
            }

            // لو فيه منتجات تانية
            _unitOfWork.Repository<HelpOrderItem, int>().Delete(item);

            await _unitOfWork.Complete();

            await _notificationService.NotifyManagersAsync(
                new NotificationMessageDto
                {
                    Title = "حذف عنصر من الطلب",
                    Message = $"تم حذف المنتج {item.Title} من الطلب رقم {helpOrderId}",
                    EntityType = nameof(HelpOrder),
                    EntityId = helpOrderId
                });

            return "تم حذف الطلب بنجاح";
        }
        public async Task<string> UpdateHelpRequestAsync(
    int itemId,
    string userId,
    int amount)
        {
            var beneficiary = await _unitOfWork.Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return "المستفيد غير موجود";

            var item = await _unitOfWork.Repository<HelpOrderItem, int>()
                .GetQueryable()
                .Include(i => i.HelpOrder)
                .FirstOrDefaultAsync(i =>
                    i.Id == itemId &&
                    i.HelpOrder.BeneficiaryId == beneficiary.Id);

            if (item == null)
                return "العنصر غير موجود";

            var helporderid = item.HelpOrderId;

            item.QuantityRequested = amount;
        
            item.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Complete();
            await _notificationService.NotifyManagersAsync(new NotificationMessageDto
            {
               // Type = NotificationType.BeneficiaryCampaignJoinRequested,
                Title = " تعديل في  كميه طلب ",
                Message = $" تعديل في  كميه طلب  رقم {helporderid}",
                EntityType = nameof(BeneficiaryCampaign),
                EntityId = item.Id
            });

            return "تم تعديل الطلب بنجاح";
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
                NationalId = beneficiary.NationalId ?? string.Empty,
                SupportFile = beneficiary.SupportFile ?? "لا يوجد اي اثيات شخصيه",

               
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



            var fileUrls = string.Empty;

            if (dto.SupportingFiles != null )
            {
                
                    var url = await _fileService.SaveFileAsync(dto.SupportingFiles, "uploads");
                fileUrls = url;
                
            }
            beneficiary.SupportFile = fileUrls;

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
