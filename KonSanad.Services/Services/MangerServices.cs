using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Dtos.ManagerDtos;
using KonSanad.Core.Entities;
using KonSanad.Core.Entity;
using KonSanad.Core.Entity.Enums;
using KonSanad.Repository.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KonSanad.Services.Services
{
    public class MangerServices : IMangerServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        public MangerServices(IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
            _notificationService = notificationService;
        }
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public async Task<string> MakeManagerAsync(int volunteerId)
        {
            // 1️⃣ هات الـ Volunteer
            var volunteer = await _unitOfWork.Repository<Volunteer, int>()
                .GetQueryable()
                .FirstOrDefaultAsync(v => v.Id == volunteerId);

            if (volunteer == null)
                return "Volunteer not found";

            // 2️⃣ هات الـ User من Identity (بالبريد مثلا)
            var user = await _userManager.FindByEmailAsync(volunteer.Email);

            if (user == null)
                return "User not found in Identity";

            // 3️⃣ تأكد إن Role موجود
            if (!await _roleManager.RoleExistsAsync("Manager"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Manager"));
            }

            // 4️⃣ شيل أي Roles قديمة (اختياري)
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // 5️⃣ أضف Role الجديد
            await _userManager.AddToRoleAsync(user, "Manager");

            return "Volunteer promoted to Manager successfully";
        }

        public async Task<string> AssignVolunteerAsync(int helpOrderId, int volunteerId, DateTime Start, DateTime End)
        {
            if (Start >= End)
                return "برجاء التأكد من تواريخ البدء والانتهاء";

            var helpOrder = await _unitOfWork.Repository<HelpOrder, int>()
                .GetQueryable()
                .Include(h => h.Beneficiary)
                .FirstOrDefaultAsync(h => h.Id == helpOrderId);

            if (helpOrder == null)
                return "الطلب غير موجود";

            if (helpOrder.Beneficiary == null)
                return "المستفيد غير موجود";

            // ✅ تحقق إن المهمة مش اتأسندت قبل كده
            var alreadyAssigned = await _unitOfWork.Repository<MissionVolunteers, int>()
                .GetQueryable()
                .Include(mv => mv.Mission)
                .AnyAsync(mv => mv.Mission.HelpOrderId == helpOrderId);

            if (alreadyAssigned)
                return "هذا الطلب تم إسناده بالفعل لمتطوع آخر";

            // ✅ جيب الـ Mission لو موجودة أو اعملها جديدة
            var mission = await _unitOfWork.Repository<Mission, int>()
                .GetQueryable()
                .FirstOrDefaultAsync(m => m.HelpOrderId == helpOrderId);

            if (mission == null)
            {
                mission = new Mission
                {
                    Title = helpOrder.Beneficiary.FullName,
                    Description = "زيارة ميدانية لمتابعة أحوال الأسر المحتاجة",
                    Address = helpOrder.Beneficiary.Address,
                    PhoneNumber = helpOrder.Beneficiary.Phone,
                    StartDate = Start,
                    EndDate = End,
                    ManagerId = 1,
                    HelpOrderId = helpOrder.Id,
                    MissionStatus = MissionStatus.Assigend,
                    Status = RequestedStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<Mission, int>().AddAsync(mission);
                await _unitOfWork.Complete();
            }

            // ✅ تأكد إن الـ Id اتولد
            if (mission.Id == 0)
                return "فشل في إنشاء المهمة";

            // ✅ Create MissionVolunteer
            var missionVolunteer = new MissionVolunteers
            {
                MissionId = mission.Id,
                VolunteerId = volunteerId,
                MissionStatus = VolunteerMissionStatus.WaitingForReport,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<MissionVolunteers, int>().AddAsync(missionVolunteer);
            await _unitOfWork.Complete();

            // ✅ Create VolunteerReport
            var volunteerReport = new VolunteerReport
            {
                VolunteerId = volunteerId,
               BeneficiaryId = helpOrder.Beneficiary.Id,
                SupportFile = helpOrder.Beneficiary.SupportFile,
                Status = RequestedStatus.Pending,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<VolunteerReport, int>().AddAsync(volunteerReport);
            await _unitOfWork.Complete();

            // ✅ Send notification
            var volunteer = await _unitOfWork.Repository<Volunteer, int>()
                .FirstOrDefaultAsync(v => v.Id == volunteerId);

            if (!string.IsNullOrWhiteSpace(volunteer?.AppUserId))
            {
                await _notificationService.NotifyUserAsync(volunteer.AppUserId, new NotificationMessageDto
                {
                    Type = NotificationType.MissionAssigned,
                    Title = "تم إسناد مهمة جديدة",
                    Message = $"تم إسناد مهمة جديدة لك تخص الطلب رقم {helpOrder.Id}.",
                    EntityType = nameof(Mission),
                    EntityId = mission.Id
                });
            }

            return "تم إسناد المهمة بنجاح";
        }

        #region HelpOrder: Ids

        public async Task<IEnumerable<int>> GetHelpOrderIdsAsync()
        {
            return await _unitOfWork.Repository<HelpOrder, int>().GetQueryable()
                .Where(h => !h.IsDeleted)
                .Select(h => h.Id)
                .ToListAsync();
        }
        #endregion
        //Get HelpOrder 

        public async Task<IEnumerable<GetHelpOrderDto>> GetHelpOrderAsync(
         int pageNumber,
         int pageSize,
         string? search,
         RequestedStatus? status,
         int? helpOrderId)
        {
            var query = _unitOfWork.Repository<HelpOrder, int>()
                .GetQueryable()
                .Include(h => h.Beneficiary)
                .Include(h => h.HelpOrderItems)
                .Where(h => !h.IsDeleted);

            // ✅ لو جاي بـ ID معين
            if (helpOrderId.HasValue)
            {
                var order = await query
                    .FirstOrDefaultAsync(h => h.Id == helpOrderId.Value);

                if (order == null)
                    return Enumerable.Empty<GetHelpOrderDto>();

                var itemsQuery = order.HelpOrderItems.AsQueryable();

                // search على items
                if (!string.IsNullOrWhiteSpace(search))
                    itemsQuery = itemsQuery.Where(i => i.Title.Contains(search));

                // filter status على items
                if (status.HasValue)
                    itemsQuery = itemsQuery.Where(i => i.HelpOrderStatus == status.Value);

                // لو مفيش items بعد الفلترة
                if (!itemsQuery.Any())
                {
                    return new List<GetHelpOrderDto>
            {
                new GetHelpOrderDto
                {
                    Id = order.Id,
                    UserName = order.Beneficiary?.FullName ?? string.Empty,
                    PhoneNumber = order.Beneficiary?.Phone ?? string.Empty,
                    Address = order.Beneficiary?.Address ?? string.Empty,
                    HelpOrderStatus = order.HelpOrderStatus,
                    CreatedAt = order.CreatedAt
                }
            };
                }

                return itemsQuery.Select(i => new GetHelpOrderDto
                {
                    Id = order.Id,
                    UserName = order.Beneficiary.FullName?? string.Empty,
                    PhoneNumber = order.Beneficiary.Phone ?? string.Empty,
                    Address = order.Beneficiary.Address ?? string.Empty,
                    HelpOrderStatus = i.HelpOrderStatus,
                    CreatedAt = order.CreatedAt,
                    Tilte = i.Title,
                    Reason = i.reason,
                    Quantity = i.QuantityRequested
                }).ToList();
            }

            // ✅ search عام
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(h =>
                    h.Beneficiary.FullName.Contains(search) ||
                    h.Beneficiary.Phone.Contains(search) ||
                    h.Beneficiary.Address.Contains(search) ||
                    h.HelpOrderItems.Any(i => i.Title.Contains(search)));
            }

            // ✅ filter status (order أو item)
            if (status.HasValue)
            {
                query = query.Where(h =>
                    h.HelpOrderStatus == status.Value ||
                    h.HelpOrderItems.Any(i => i.HelpOrderStatus == status.Value));
            }

            // ✅ pagination بعد كل الفلترة
            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ✅ mapping
            var result = orders.SelectMany(h =>
                h.HelpOrderItems != null && h.HelpOrderItems.Any()
                    ? h.HelpOrderItems.Select(i => new GetHelpOrderDto
                    {
                        Id = h.Id,
                        UserName = h.Beneficiary?.FullName ?? string.Empty,
                        PhoneNumber = h.Beneficiary?.Phone ?? string.Empty,
                        Address = h.Beneficiary?.Address ?? string.Empty,
                        HelpOrderStatus = i.HelpOrderStatus,
                        CreatedAt = h.CreatedAt,
                        Tilte = i.Title,
                        Reason = i.reason,
                        Quantity = i.QuantityRequested
                    })
                    : new List<GetHelpOrderDto>
                    {
                new GetHelpOrderDto
                {
                    Id = h.Id,
                    UserName = h.Beneficiary?.FullName ?? string.Empty,
                    PhoneNumber = h.Beneficiary?.Phone ?? string.Empty,
                    Address = h.Beneficiary?.Address ?? string.Empty,
                    HelpOrderStatus = h.HelpOrderStatus,
                    CreatedAt = h.CreatedAt
                }
                    }
            );

            return result.ToList();
        }  // Get Volunteer in Charity
        public async Task<IEnumerable<GetVolunteerDto>> GetVolunteerAsync(
      string? search = null,          // بيسيرش في كل الحقول
      RequestedStatus? status = null, // الفيلتر بالحالة بس
      int pageNumber = 1,
      int pageSize = 8)
        {
            var query = _unitOfWork.Repository<Volunteer, int>().GetQueryable()
                .Where(v => !v.IsDeleted);

            // فيلتر بالحالة (مرفوض / قيد الانتظار / موافق عليه)
            if (status.HasValue)
                query = query.Where(v => v.Status == status.Value);

            // سيرش في كل الحقول المتاحة
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(v =>
                    v.FullName.Contains(search) ||  // بيسيرش في الاسم
                    v.Address.Contains(search) ||  // بيسيرش في العنوان
                    v.Skills.Contains(search) ||  // بيسيرش في المهارة
                    v.Availability.Contains(search));   // بيسيرش في التفرغ

            var result = await query
                .OrderByDescending(v => v.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new GetVolunteerDto
                {
                    Id = v.Id,
                    Name = v.FullName,
                    Location = v.Address,
                    Skills = v.Skills,
                    Availability = v.Availability.ToString(),
                    Status = (RequestedStatus)v.Status
                })
                .ToListAsync();

            return result;
        }

        public async Task<string> RejectOrder(int missionId )
        {
            var mission = await _unitOfWork.Repository<Mission, int>()
               .GetQueryable()
               .Include(m => m.HelpOrder)
                   .ThenInclude(h => h.Beneficiary)
               .Include(m => m.HelpOrder)
                   .ThenInclude(h => h.HelpOrderItems)
               .FirstOrDefaultAsync(m => m.Id == missionId);

            if (mission == null)
                return "المهمة غير موجودة";
            var repo = await _unitOfWork.Repository<VolunteerReport, int>()
              .FirstOrDefaultAsync(r => r.BeneficiaryId == mission.HelpOrder.BeneficiaryId);


            var vm = await _unitOfWork.Repository<MissionVolunteers, int>()
                .GetQueryable()
                .FirstOrDefaultAsync(mv => mv.MissionId == missionId);

            if (vm == null)
                return "المهمة التطوعية غير موجودة";

            // ✅ Update mission
            mission.MissionStatus = MissionStatus.Completed;
            mission.HelpOrder.HelpOrderItems.FirstOrDefault().HelpOrderStatus = RequestedStatus.Rejected; // لو عايز ترفض أول item
            mission.HelpOrder.HelpOrderItems.FirstOrDefault().Status = RequestedStatus.Rejected; // لو عندك حقل Status في HelpOrderItem نفسه
            mission.HelpOrder.HelpOrderItems.FirstOrDefault().UpdatedAt = DateTime.Now; // لو عايز تحدث وقت التحديث
            mission.HelpOrder.ManagerId = 1; // Assign to manager with ID 1 (or any logic you want)
            mission.HelpOrder.Status = RequestedStatus.Rejected; // لو عندك حقل Status في HelpOrder نفسه
            mission.HelpOrder.UpdatedAt = DateTime.Now;
            repo.SupportFile = vm.ResultMission;
            // ✅ Update volunteer mission
            vm.MissionStatus = VolunteerMissionStatus.Completed;

            // ✅ Update help order
            mission.HelpOrder.HelpOrderStatus = RequestedStatus.Rejected;

            // ✅ Update first item (لو موجود)
            var item = mission.HelpOrder.HelpOrderItems.FirstOrDefault();
            if (item != null)
                item.Status = RequestedStatus.Rejected;

            // ✅ Save changes
            await _unitOfWork.Complete();

            if (!string.IsNullOrWhiteSpace(mission.HelpOrder?.Beneficiary?.AppUserId))
            {
                await _notificationService.NotifyUserAsync(mission.HelpOrder.Beneficiary.AppUserId, new NotificationMessageDto
                {
                    Type = NotificationType.HelpRequestRejected,
                    Title = "تم رفض الطلب",
                    Message = $"تم رفض طلبك رقم {mission.HelpOrder.Id}.",
                    EntityType = nameof(HelpOrder),
                    EntityId = mission.HelpOrder.Id
                });
            }

            return "تم رفض الطلب ";

        }
        public async Task<string> AcceptOrder(int missionId)
        {
            var mission = await _unitOfWork.Repository<Mission, int>()
                .GetQueryable()
                .Include(m => m.HelpOrder)
                    .ThenInclude(h => h.Beneficiary)
                .Include(m => m.HelpOrder)
                    .ThenInclude(h => h.HelpOrderItems)
                .FirstOrDefaultAsync(m => m.Id == missionId);

            if (mission == null)
                return "المهمة غير موجودة";

            if (mission.HelpOrder == null)
                return "لا يوجد طلب مرتبط بهذه المهمة";

            var vm = await _unitOfWork.Repository<MissionVolunteers, int>()
                .GetQueryable()
                .FirstOrDefaultAsync(mv => mv.MissionId == missionId);

            if (vm == null)
                return "المهمة التطوعية غير موجودة";

            var repo = await _unitOfWork.Repository<VolunteerReport, int>()
                .FirstOrDefaultAsync(r => r.BeneficiaryId == mission.HelpOrder.BeneficiaryId);

            if (repo == null)
                return "لا يوجد تقرير للمتطوع";

            var supItem = mission.HelpOrder.HelpOrderItems.FirstOrDefault();

            if (supItem == null)
                return "لا يوجد عناصر في الطلب";

            var supply = await _unitOfWork.Repository<Supply, int>()
                .FirstOrDefaultAsync(s => s.Id == supItem.SupplyId);

            if (supply == null)
                return "العنصر غير موجود في المخزن";

            // ✅ Update mission
            mission.MissionStatus = MissionStatus.Completed;

            // ✅ Update volunteer mission
            vm.MissionStatus = VolunteerMissionStatus.Completed;
            repo.SupportFile = vm.ResultMission;

            // ✅ Update help order
            mission.HelpOrder.HelpOrderStatus = RequestedStatus.Approved;
            mission.HelpOrder.ManagerId = 1;
            mission.HelpOrder.Status = RequestedStatus.Approved;
            mission.HelpOrder.UpdatedAt = DateTime.UtcNow;

            // ✅ Update stock
            supply.StockQuantity -= supItem.QuantityRequested;

            // ✅ Update item
            supItem.HelpOrderStatus = RequestedStatus.Approved;
            supItem.Status = RequestedStatus.Approved;
            supItem.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Complete();

            // ✅ Notification
            if (!string.IsNullOrWhiteSpace(mission.HelpOrder?.Beneficiary?.AppUserId))
            {
                await _notificationService.NotifyUserAsync(
                    mission.HelpOrder.Beneficiary.AppUserId,
                    new NotificationMessageDto
                    {
                        Type = NotificationType.HelpRequestApproved,
                        Title = "تمت الموافقة على الطلب",
                        Message = $"تمت الموافقة على طلبك رقم {mission.HelpOrder.Id}.",
                        EntityType = nameof(HelpOrder),
                        EntityId = mission.HelpOrder.Id
                    });
            }

            return "تم قبول الطلب بنجاح";
        }
        public async Task<ViewReportDto> ViewReport(int helpOrderId)
        {
            var helpOrder = await _unitOfWork.Repository<HelpOrder, int>()
.GetQueryable()
.Include(h => h.Beneficiary)  //غيرت ال status ال فجدول المستفيدين فالداتا بيز ل int <== xxx
.FirstOrDefaultAsync(h => h.Id == helpOrderId);

            var mission = await _unitOfWork.Repository<Mission, int>()
                .GetQueryable()
                .Include(m => m.HelpOrder)
                    .ThenInclude(h => h.Beneficiary)
                .Include(m => m.HelpOrder)
                    .ThenInclude(h => h.HelpOrderItems)
                  .FirstOrDefaultAsync(m => m.HelpOrderId == helpOrderId && m.Title == helpOrder.Beneficiary.FullName && m.PhoneNumber == helpOrder.Beneficiary.Phone);


            if (mission == null)
                return  null;

            var report = await _unitOfWork.Repository<MissionVolunteers, int>()
                .GetQueryable()
                .Where(vm => vm.MissionId == mission.Id)
                .Select(vm => new ViewReportDto
                {
                    BeneficiaryName = mission.HelpOrder.Beneficiary.FullName,

                    // 👇 لو عايز أول Title من الـ items
                    Title = mission.HelpOrder.HelpOrderItems
                                .Select(i => i.Title)
                                .FirstOrDefault(),
                    missionId = mission.Id,

                    VolunteerName = vm.Volunteer.FullName,
                    Notes = vm.Notes,
                    FileUrl = vm.ResultMission
                })
                .FirstOrDefaultAsync();

            return report;
        }

        public async Task<IEnumerable<GetJoinVolunteerDto>> GetJoinVolunteer(int pageNumber, int pageSize, RequestedStatus? status) // ✅ UPDATED: استقبلنا status
        {
            var query = _unitOfWork.Repository<Volunteer, int>()
                .GetQueryable()
                .Where(v => !v.IsDeleted); // existing condition

            // ✅ NEW: فلترة على الـ status لو المستخدم بعت قيمة
            if (status.HasValue)
            {
                query = query.Where(v => v.Status == status.Value);
            }
            //-------------------------------------------------------
            var volunteers = await query // ✅ UPDATED: بقينا نشتغل على query بدل ما كنا شغالين مباشرة
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new GetJoinVolunteerDto
                {
                    Id = v.Id,
                    Status = (RequestedStatus)v.Status,
                    Email = v.Email,
                    JoinDate = v.CreatedAt,
                    Name = v.FullName,
                    Location = v.Address,
                    Skills = v.Skills,
                    Availability = v.Availability
                })
                .ToListAsync();

            return volunteers;
        }

        public async Task<string> AcceptVolunteer(int VolId)
        {
            var volunteer = _unitOfWork.Repository<Volunteer, int>()
                 .GetQueryable()
                 .FirstOrDefault(v => v.Id == VolId);
            if (volunteer == null)
                return "المتطوع غير موجود";
            volunteer.Status = RequestedStatus.Approved;
            volunteer.ManagerId = 1; // Assign to manager with ID 1 (or any logic you want)
            _unitOfWork.Repository<Volunteer, int>().Update(volunteer);
            await _unitOfWork.Complete();

            if (!string.IsNullOrWhiteSpace(volunteer.AppUserId))
            {
                await _notificationService.NotifyUserAsync(volunteer.AppUserId, new NotificationMessageDto
                {
                    Type = NotificationType.VolunteerRegistrationApproved,
                    Title = "تم قبول طلب التطوع",
                    Message = "تم قبول طلب التطوع الخاص بك من المدير.",
                    EntityType = nameof(Volunteer),
                    EntityId = volunteer.Id
                });
            }

            return "تم قبول طلب التطوع بنجاح";
        }

        public async Task<string> RejectVolunteer(int VolId , string Reason)
        {
            var volunteer = _unitOfWork.Repository<Volunteer, int>()
                 .GetQueryable()
                 .FirstOrDefault(v => v.Id == VolId);
            if (volunteer == null)
                return "المتطوع غير موجود";
            volunteer.Status = RequestedStatus.Rejected;
            volunteer.ReajectReason = Reason;
            volunteer.ManagerId = 1; // Assign to manager with ID 1 (or any logic you want)
            _unitOfWork.Repository<Volunteer, int>().Update(volunteer);
            await _unitOfWork.Complete();

            if (!string.IsNullOrWhiteSpace(volunteer.AppUserId))
            {
                await _notificationService.NotifyUserAsync(volunteer.AppUserId, new NotificationMessageDto
                {
                    Type = NotificationType.VolunteerRegistrationRejected,
                    Title = "تم رفض طلب التطوع",
                    Message = $"تم رفض طلب التطوع الخاص بك. السبب: {Reason}",
                    EntityType = nameof(Volunteer),
                    EntityId = volunteer.Id
                });
            }

            return "تم رفض طلب التطوع ";

        }
        // Get All Volunteer Requests [ Join a campaigns + Start Mission ]
        public async Task<IEnumerable<GetVolunteerRequestsDto>> GetAllVolunteerRequestsAsync(
      int pageNumber,
      int pageSize,
      string? search = null,
      RefType? requestType = null,
      VolunteerMissionStatus? missionStatus = null)
        {
            var result = new List<GetVolunteerRequestsDto>();

            // ============================================================
            // 1️⃣ Campaign Requests
            // بنجيبهم بس لو الفيلتر الأول مش محدد أو محدد بـ Campaign
            // ============================================================
            if (requestType == null || requestType == RefType.Campaign)
            {
                var campaignRequests = await _unitOfWork.Repository<CampaignVolunteer, int>()
                    .GetQueryable()
                    .Where(cv => !cv.IsDeleted &&
                        (cv.Status == RequestedStatus.Pending ||
                         cv.Status == RequestedStatus.Rejected ||
                         cv.Status == RequestedStatus.Approved))
                    .Include(cv => cv.Volunteer)
                    .Include(cv => cv.Campaign)
                    .Select(cv => new GetVolunteerRequestsDto
                    {
                        RequestType = RefType.Campaign,
                        ReferenceId = cv.Id,
                        VolunteerId = cv.VolunteerId,
                        VolunteerName = cv.Volunteer.FullName,
                        Address = cv.Volunteer.Address,
                        Phone = cv.Volunteer.Phone,
                        Title = cv.Campaign.Title,
                        Status = (int)cv.Status,
                        CreatedAt = cv.CreatedAt
                    })
                    .ToListAsync();

                result.AddRange(campaignRequests);
            }

            // ============================================================
            // 2️⃣ Mission Requests
            // بنجيبهم بس لو الفيلتر الأول مش محدد أو محدد بـ Mission
            // ============================================================
            if (requestType == null || requestType == RefType.Mission)
            {
                var missionRequests = await _unitOfWork.Repository<MissionVolunteers, int>()
                    .GetQueryable()
                    .Where(mv => !mv.IsDeleted &&
                        (mv.MissionStatus == VolunteerMissionStatus.InReview ||
                         mv.MissionStatus == VolunteerMissionStatus.Rejected ||
                         mv.MissionStatus == VolunteerMissionStatus.WaitingForReport ||
                         mv.Mission.MissionStatus == MissionStatus.Assigend))
                    .Include(mv => mv.Volunteer)
                    .Include(mv => mv.Mission)
                    .Select(mv => new GetVolunteerRequestsDto
                    {
                        RequestType = RefType.Mission,
                        ReferenceId = mv.Id,
                        VolunteerId = mv.VolunteerId,
                        VolunteerName = mv.Volunteer.FullName,
                        Address = mv.Volunteer.Address,
                        Phone = mv.Volunteer.Phone,
                        Title = mv.Mission.Title,
                        Status = (int)mv.MissionStatus,
                        CreatedAt = mv.CreatedAt
                    })
                    .ToListAsync();

                result.AddRange(missionRequests);
            }

            // ============================================================
            // 3️⃣ فيلتر الحالة (VolunteerMissionStatus)
            // بيتعمل بعد الجمع لأن البيانات جاية من جدولين مختلفين
            // لو المدير اختار حالة معينة بيفلتر بيها، لو null بيجيب الكل
            // ============================================================
            if (missionStatus.HasValue)
                result = result
                    .Where(r => r.Status == (int)missionStatus.Value)
                    .ToList();

            // ============================================================
            // 4️⃣ السيرش في كل حقول الـ DTO
            // بيتعمل بعد الفيلتر لأن البيانات من جدولين
            // بيسيرش في: الاسم، التليفون، العنوان، اسم الحملة/المهمة
            // ============================================================
            if (!string.IsNullOrWhiteSpace(search))
                result = result.Where(r =>
                    r.VolunteerName.Contains(search, StringComparison.OrdinalIgnoreCase) || // سيرش في اسم المتطوع
                    r.Phone.Contains(search, StringComparison.OrdinalIgnoreCase) || // سيرش في التليفون
                    r.Address.Contains(search, StringComparison.OrdinalIgnoreCase) || // سيرش في العنوان
                    r.Title.Contains(search, StringComparison.OrdinalIgnoreCase)  // سيرش في اسم الحملة أو المهمة
                                           
                ).ToList();

            // ============================================================
            // 5️⃣ Pagination + Sorting
            // بيتعمل في الآخر بعد الفيلتر والسيرش
            // ============================================================
            result = result
                .OrderByDescending(r => r.CreatedAt)        // ترتيب من الأحدث للأقدم
                .Skip((pageNumber - 1) * pageSize)          // تخطي صفحات السابقة
                .Take(pageSize)                             // أخد عدد العناصر المطلوبة
                .ToList();

            return result;
        }

        public async Task<string> AcceptRequst(int requstType, int referenceId)
        {
            // 🔹 Campaign Request
            if (requstType == (int)RefType.Campaign)
            {
                var campaignRequest = await _unitOfWork.Repository<CampaignVolunteer, int>()
                    .GetQueryable()
                    .Include(c => c.Campaign)
                    .Include(c => c.Volunteer)
                    .FirstOrDefaultAsync(c => c.Id == referenceId);

                if (campaignRequest == null)
                    return "طلب الحملة غير موجود";

                campaignRequest.Status = RequestedStatus.Approved;

                await _unitOfWork.Complete();

                if (!string.IsNullOrWhiteSpace(campaignRequest.Volunteer?.AppUserId))
                {
                    await _notificationService.NotifyUserAsync(campaignRequest.Volunteer.AppUserId, new NotificationMessageDto
                    {
                        Type = NotificationType.VolunteerCampaignJoinApproved,
                        Title = "تم قبول طلبك",
                        Message = $"تم قبول طلب انضمامك إلى الحملة {campaignRequest.Campaign?.Title}.",
                        EntityType = nameof(CampaignVolunteer),
                        EntityId = campaignRequest.Id
                    });
                }

                return "تم قبول طلب الحملة بنجاح";
            }

            // 🔹 Mission Request
            else if (requstType == (int)RefType.Mission)
            {
                var missionRequest = await _unitOfWork.Repository<MissionVolunteers, int>()
                    .GetQueryable()
                    .Include(mv => mv.Volunteer)
                    .Include(mv => mv.Mission)
                        .ThenInclude(m => m.HelpOrder)
                    .FirstOrDefaultAsync(mv => mv.Id == referenceId);

                if (missionRequest == null)
                    return "طلب المهمة غير موجود";

                if (missionRequest.Mission.MissionStatus == MissionStatus.Assigend)
                {                  
                    return "تم قبول هذه المهمة بالفعل من قبل متطوع آخر";
                }


                // ✅ Update volunteer mission
                missionRequest.MissionStatus = VolunteerMissionStatus.WaitingForReport;

                // ✅ ممكن كمان تحدّث المهمة نفسها
                if (missionRequest.Mission != null)
                {
                    missionRequest.Mission.MissionStatus = MissionStatus.Assigend;
                }


                await _unitOfWork.Complete();

                if (!string.IsNullOrWhiteSpace(missionRequest.Volunteer?.AppUserId))
                {
                    await _notificationService.NotifyUserAsync(missionRequest.Volunteer.AppUserId, new NotificationMessageDto
                    {
                        Type = NotificationType.MissionRequestApproved,
                        Title = "تم قبول طلب المهمة",
                        Message = $"تم قبول طلبك لبدء المهمة رقم {missionRequest.MissionId}.",
                        EntityType = nameof(MissionVolunteers),
                        EntityId = missionRequest.Id
                    });
                }

                return "تم قبول طلب المهمة بنجاح";
            }

            return "نوع الطلب غير صالح";
        }

        public async Task<string> RejectRequst(int requstType, int referenceId , string Reason)
        {
            // 🔹 Campaign Request
            if (requstType == (int)RefType.Campaign)
            {
                var campaignRequest = await _unitOfWork.Repository<CampaignVolunteer, int>()
                    .GetQueryable()
                    .Include(c => c.Campaign)
                    .Include(c => c.Volunteer)
                    .FirstOrDefaultAsync(c => c.Id == referenceId);

                if (campaignRequest == null)
                    return "طلب الحملة غير موجود";

                // ✅ منع التكرار
                if (campaignRequest.Status != RequestedStatus.Pending)
                    return "تم معالجة الطلب بالفعل";
                campaignRequest.Status = RequestedStatus.Rejected;
                campaignRequest.ReajectReason = Reason;

                await _unitOfWork.Complete();

                if (!string.IsNullOrWhiteSpace(campaignRequest.Volunteer?.AppUserId))
                {
                    await _notificationService.NotifyUserAsync(campaignRequest.Volunteer.AppUserId, new NotificationMessageDto
                    {
                        Type = NotificationType.VolunteerCampaignJoinRejected,
                        Title = "تم رفض الطلب",
                        Message = $"تم رفض طلب انضمامك إلى الحملة {campaignRequest.Campaign?.Title}. السبب: {Reason}",
                        EntityType = nameof(CampaignVolunteer),
                        EntityId = campaignRequest.Id
                    });
                }

                return "تم رفض طلب الحملة ";
            }

            // 🔹 Mission Request
            else if (requstType == (int)RefType.Mission)
            {
                var missionRequest = await _unitOfWork.Repository<MissionVolunteers, int>()
                    .GetQueryable()
                    .Include(mv => mv.Volunteer)
                    .Include(mv => mv.Mission)
                    .FirstOrDefaultAsync(mv => mv.Id == referenceId);

                if (missionRequest == null)
                    return "طلب المهمة غير موجود";

                // ✅ منع التكرار
                if (missionRequest.MissionStatus != VolunteerMissionStatus.InReview)
                    return "تم معالجة الطلب بالفعل";
                // ✅ تحديث حالة المتطوع
                missionRequest.MissionStatus = VolunteerMissionStatus.Rejected;

                // (اختياري) ترجع المهمة Pending أو تسيبها زي ما هي
                if (missionRequest.Mission != null)
                {
                    missionRequest.Mission.MissionStatus = MissionStatus.New;
                }

                await _unitOfWork.Complete();

                if (!string.IsNullOrWhiteSpace(missionRequest.Volunteer?.AppUserId))
                {
                    await _notificationService.NotifyUserAsync(missionRequest.Volunteer.AppUserId, new NotificationMessageDto
                    {
                        Type = NotificationType.MissionRequestRejected,
                        Title = "تم رفض طلب المهمة",
                        Message = $"تم رفض طلبك لبدء المهمة رقم {missionRequest.MissionId}. السبب: {Reason}",
                        EntityType = nameof(MissionVolunteers),
                        EntityId = missionRequest.Id
                    });
                }

                return "تم رفض طلب المهمة بنجاح";
            }

            return "نوع الطلب غير صالح";
        }
        //-----------------------------------------------------------------------------------------------------
        public async Task<VolunteerRequestDetailDto> GetRequestDetailsAsync(
int referenceId, int type)
        {
            if (type == 1)
            {
                var cv = await _unitOfWork.Repository<CampaignVolunteer, int>()
                    .GetQueryable()
                    .Where(cv => cv.Id == referenceId && !cv.IsDeleted)
                    .Include(cv => cv.Volunteer)
                        .ThenInclude(v => v.VolunteerMissions)
                    .Include(cv => cv.Volunteer)
                        .ThenInclude(v => v.CampaignVolunteers)
                    .Include(cv => cv.Campaign)
                    .FirstOrDefaultAsync();
                /*
                                if (cv == null)
                                    throw new NoContentResult("Request not found.");
                */
                return new VolunteerRequestDetailDto
                {
                    RequestType = RefType.Campaign,
                    ReferenceId = cv.Id,
                    Status = (int)cv.Status,
                    CreatedAt = cv.CreatedAt,

                    // Volunteer
                    VolunteerId = cv.VolunteerId,
                    VolunteerName = cv.Volunteer.FullName,
                    Email = cv.Volunteer.Email,
                    Phone = cv.Volunteer.Phone,
                    Address = cv.Volunteer.Address,

                    TotalMissions = cv.Volunteer.VolunteerMissions
                                            .Count(m => !m.IsDeleted),
                    TotalCampaigns = cv.Volunteer.CampaignVolunteers
                                            .Count(c => !c.IsDeleted),

                    // Campaign
                    Title = cv.Campaign.Title,
                    Description = cv.Campaign.Description,
                    StartDate = cv.Campaign.StartDate,
                    EndDate = cv.Campaign.EndDate,

                    CampaignJoinStatus = cv.Status,
                    CampaignVolunteersCount = cv.Campaign.CampaignVolunteers
                                                .Count(x => !x.IsDeleted),
                };
            }
            else // RefType.Mission
            {
                var mv = await _unitOfWork.Repository<MissionVolunteers, int>()
                    .GetQueryable()
                    .Where(mv => mv.Id == referenceId && !mv.IsDeleted)
                    .Include(mv => mv.Volunteer)
                        .ThenInclude(v => v.VolunteerMissions)
                    .Include(mv => mv.Volunteer)
                        .ThenInclude(v => v.CampaignVolunteers)
                    .Include(mv => mv.Mission)
                    .FirstOrDefaultAsync();
                /*
                                if (mv == null)
                                    throw new NotFoundException("Request not found.");
                */
                return new VolunteerRequestDetailDto
                {
                    RequestType = RefType.Mission,
                    ReferenceId = mv.Id,
                    Status = (int)mv.MissionStatus,
                    CreatedAt = mv.CreatedAt,

                    // Volunteer
                    VolunteerId = mv.VolunteerId,
                    VolunteerName = mv.Volunteer.FullName,
                    Email = mv.Volunteer.Email,
                    Phone = mv.Volunteer.Phone,
                    Address = mv.Volunteer.Address,
                    TotalMissions = mv.Volunteer.VolunteerMissions
                                            .Count(m => !m.IsDeleted),
                    TotalCampaigns = mv.Volunteer.CampaignVolunteers
                                            .Count(c => !c.IsDeleted),

                    // Mission
                    Title = mv.Mission.Title,
                    Description = mv.Mission.Description,
                    StartDate = mv.Mission.StartDate,
                    EndDate = mv.Mission.EndDate,
                    MissionStatus = mv.MissionStatus,

                };
            }
        }
        //-------------------------------------------------------------------------------------------------



        #region Donation
        public async Task<IEnumerable<GetAllDonationDto>> GetDonationAsync(
     int pageNumber,
     int pageSize,
     string? search = null,
     DonationCategory? category = null,
     RequestedStatus? status = null)
        {
            var result = new List<GetAllDonationDto>();

            // ============================================================
            // 1️⃣ Cash Donations
            // بنجيبهم بس لو الفيلتر مش محدد أو محدد بـ Cash
            // ============================================================
            if (category == null || category == DonationCategory.Cash)
            {
                var cashDonations = await _unitOfWork.Repository<CashDonation, int>()
                    .GetQueryable()
                    .Include(cd => cd.DonationOrder)
                    .ThenInclude(d => d.Donor)
                    .Where(cd => !cd.IsDeleted)

                    // فيلتر الحالة داخل الـ Query على الـ Database مباشرة
                    // لو status محدد بيفلتر، لو null بيجيب الكل
                    .Where(cd => status == null || (int)cd.cashDonationStatus == (int)status)

                    .Select(cd => new GetAllDonationDto
                    {
                        Id = cd.DonationOrder.Id,
                        UserName = cd.DonationOrder.Donor.FullName,
                        DonationStatus = (RequestedStatus)cd.cashDonationStatus,
                        DonationCategory = DonationCategory.Cash,
                        CreatedAt = cd.CreatedAt,
                        AmountCashDonation = cd.Amount,

                        // جلب اسم الحملة لو التبرع مرتبط بحملة
                        CampaignTitle = cd.CampaignId != 0
                            ? _unitOfWork.Repository<Campaign, int>()
                                .GetQueryable()
                                .Where(c => c.Id == cd.CampaignId)
                                .Select(c => c.Title)
                                .FirstOrDefault()
                            : null,

                        // جلب اسم المنتج لو التبرع مش مرتبط بحملة
                        SupplyName = cd.CampaignId == 0 && cd.HelpOrderId != 0
                            ? _unitOfWork.Repository<Supply, int>()
                                .GetQueryable()
                                .Where(s => s.Id == cd.HelpOrderId)
                                .Select(s => s.Name)
                                .FirstOrDefault()
                            : null,

                        Quantity = 0  // التبرع النقدي مفيهوش كمية
                    })
                    .ToListAsync();

                result.AddRange(cashDonations);
            }

            // ============================================================
            // 2️⃣ Item Donations
            // بنجيبهم بس لو الفيلتر مش محدد أو محدد بـ Item
            // ============================================================
            if (category == null || category == DonationCategory.Item)
            {
                var itemDonations = await _unitOfWork.Repository<OrderItem, int>()
                    .GetQueryable()
                    .Include(oi => oi.DonationOrder)
                        .ThenInclude(d => d.Donor)
                    .Where(oi => !oi.IsDeleted)

                    // فيلتر الحالة داخل الـ Query على الـ Database مباشرة
                    .Where(oi => status == null || (int)oi.orderItemStatus == (int)status)

                    .Select(oi => new GetAllDonationDto
                    {
                        Id = oi.DonationOrder.Id,
                        UserName = oi.DonationOrder.Donor.FullName,
                        DonationStatus = oi.orderItemStatus != null
                                         ? (RequestedStatus)oi.orderItemStatus
                                         : RequestedStatus.Pending,
                        DonationCategory = DonationCategory.Item,
                        CreatedAt = oi.CreatedAt,
                        SupplyId = oi.SupplyId.HasValue ? oi.SupplyId.Value : 0,
                        SupplyName = oi.CustomSupplyName ?? "",
                        Quantity = oi.Quantity,
                        AmountCashDonation = 0,  // تبرع عيني مفيهوش مبلغ
                        CampaignTitle = null
                    })
                    .ToListAsync();

                result.AddRange(itemDonations);
            }

            // ============================================================
            // 3️⃣ Search (بيتعمل in-memory بعد الجمع لأن البيانات من جدولين)
            // بيسيرش في: اسم المتبرع، اسم المنتج، اسم الحملة، المبلغ
            // ============================================================
            if (!string.IsNullOrWhiteSpace(search))
                result = result.Where(r =>
                    r.UserName.Contains(search, StringComparison.OrdinalIgnoreCase) ||        // سيرش في اسم المتبرع
                    r.SupplyName.Contains(search, StringComparison.OrdinalIgnoreCase) ||      // سيرش في اسم المنتج
                    (r.CampaignTitle != null && r.CampaignTitle.Contains(search, StringComparison.OrdinalIgnoreCase)) || // سيرش في اسم الحملة
                    r.AmountCashDonation.ToString().Contains(search)                          // سيرش في المبلغ
                ).ToList();

            // ============================================================
            // 4️⃣ Sorting + Pagination (بيتعمل في الآخر بعد الفيلتر والسيرش)
            // ============================================================
            return result
                .OrderByDescending(d => d.CreatedAt)       // ترتيب من الأحدث للأقدم
                .Skip((pageNumber - 1) * pageSize)         // تخطي الصفحات السابقة
                .Take(pageSize)                            // أخد عدد العناصر المطلوبة
                .ToList();
        }


        //---------------------------------------------------------------------------------------
        public async Task<GetAllDonationDetailsDto> ViewAllDetailsDonation(int DonationOrderId)
        {
            // ======= Try Cash Donation first =======
            var cashDonation = await _unitOfWork.Repository<CashDonation, int>()
                .GetQueryable()
                .Include(cd => cd.DonationOrder)
                    .ThenInclude(d => d.Donor)
                .Where(cd => !cd.IsDeleted && cd.DonationOrder.Id == DonationOrderId) //هات التبرع اللي: مش محذوف , رقم الطلب بتاعه = الـ id اللي جايلي.
                .FirstOrDefaultAsync(); 

            if (cashDonation != null)
            {
                // Base fields
                var dto = new GetAllDonationDetailsDto
                {
                    Id = cashDonation.DonationOrder.Id,
                    UserName = cashDonation.DonationOrder.Donor.FullName,
                    DonationStatus = (RequestedStatus)cashDonation.cashDonationStatus,
                    DonationCategory = DonationCategory.Cash,
                    CreatedAt = cashDonation.CreatedAt,
                    AmountCashDonation = cashDonation.Amount,
                    SupplyId = cashDonation.HelpOrderId.HasValue ? cashDonation.HelpOrderId.Value : 0,
                    Quantity = 0,
                };

                // If linked to a Campaign → get campaign details
                if (cashDonation.CampaignId != 0 && cashDonation.CampaignId != null)
                {
                    var campaign = await _unitOfWork.Repository<Campaign, int>()
                        .GetQueryable()
                        .Where(c => c.Id == cashDonation.CampaignId)
                        .FirstOrDefaultAsync();

                    if (campaign != null)
                    {
                        dto.CampaignTitle = campaign.Title;
                        dto.CampaignDescription = campaign.Description;
                        dto.StartDate = campaign.StartDate;
                        dto.EndDate = campaign.EndDate;
                        dto.ImageFile = campaign.PictureUrl;
                    }
                }
                // If linked to a Supply (HelpOrder) → get supply name
                else if (cashDonation.HelpOrderId != null && cashDonation.HelpOrderId != 0)
                {
                    var supply = await _unitOfWork.Repository<Supply, int>()
                        .GetQueryable()
                        .Where(s => s.Id == cashDonation.HelpOrderId)
                        .FirstOrDefaultAsync();

                    dto.SupplyName = supply?.Name ?? string.Empty;
                    dto.ImageFile = supply?.ImageUrl ?? string.Empty; //الصور فجدول supply في الداتا بيز بايظه
       
                }

                return dto;
            }

            // ======= Try Item Donation =======
            var orderItem = await _unitOfWork.Repository<OrderItem, int>()
                .GetQueryable()
                .Include(oi => oi.DonationOrder)
                    .ThenInclude(d => d.Donor)
                .Where(oi => !oi.IsDeleted && oi.DonationOrder.Id == DonationOrderId)
                .FirstOrDefaultAsync();

            if (orderItem != null)
            {
                var dto = new GetAllDonationDetailsDto
                {
                    Id = orderItem.DonationOrder.Id,
                    UserName = orderItem.DonationOrder.Donor.FullName,
                    DonationStatus = orderItem.orderItemStatus != null
    ? (RequestedStatus)orderItem.orderItemStatus
    : RequestedStatus.Pending,
                    DonationCategory = DonationCategory.Item,
                    CreatedAt = orderItem.CreatedAt,
                    SupplyId = orderItem.SupplyId ?? 0,
                    SupplyName = orderItem.CustomSupplyName ?? string.Empty,
                    Quantity = orderItem.Quantity,
                    AmountCashDonation = 0,
                    CampaignTitle = null,

                    // Extra details from OrderItem
                    ImageFile = orderItem.ImageUrl,
                    Unit = orderItem.Unit,
                    SupplyDescription = orderItem.Description,
                    SupplyStatus = orderItem.StatusPro,

                    RejectReason = orderItem.RejectReason,
                };

                return dto;
            }

            // ======= Not Found =======
            return null;
        }
        //---------------------------------------------------------------------------------------

        public async Task<string> AcceptDonation(int DonationOrderId)
        {
            var donationOrder = await _unitOfWork.Repository<DonationOrder, int>()
                .GetQueryable()
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(d => d.Id == DonationOrderId);
            if (donationOrder == null)
                return "التبرع مش موجود";

            var orderItems = await _unitOfWork.Repository<OrderItem, int>()
                .GetQueryable()
                .Where(oi => oi.DonationOrderId == DonationOrderId && !oi.IsDeleted)
                .ToListAsync();

            if (orderItems == null || !orderItems.Any())
                return "مفيش items مرتبطة بالتبرع ده";

            if (orderItems.Any(oi => oi.orderItemStatus == RequestedStatus.Rejected))
                return "مينفعش توافق على تبرع اترفض قبل كده";

            if (donationOrder.DonationCategoryStatus == "custom")
            {
                foreach (var item in orderItems)
                {
                    item.orderItemStatus = RequestedStatus.Approved;

                    var supply = new Supply
                    {
                        Name = item.CustomSupplyName,
                        Description = item.Description,
                        Unit = item.Unit,
                        StockQuantity = item.Quantity,
                        MinimumLevel = 5,
                        Category = null,
                        Status = RequestedStatus.Approved,
                        IsDeleted = false,
                        ImageUrl = item.ImageUrl,
                        Title = "غذائي",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                    };

                    await _unitOfWork.Repository<Supply, int>().AddAsync(supply);
                }
            }
            else if (donationOrder.DonationCategoryStatus == "donationItem")
            {
                foreach (var item in orderItems)
                {
                    item.orderItemStatus = RequestedStatus.Approved;

                    var supply = await _unitOfWork.Repository<Supply, int>()
                        .GetQueryable()
                        .FirstOrDefaultAsync(s => s.Id == item.SupplyId); // ✅ جيب الـ Supply الموجود

                    if (supply == null)
                        return $"الـ Supply مش موجود للـ item رقم {item.Id}";

                    supply.StockQuantity += item.Quantity; // ✅ زود الكمية
                    supply.UpdatedAt = DateTime.Now;
                }
            }
            

            await _unitOfWork.Complete();

            if (!string.IsNullOrWhiteSpace(donationOrder.Donor?.AppUserId))
            {
                await _notificationService.NotifyUserAsync(donationOrder.Donor.AppUserId, new NotificationMessageDto
                {
                    Type = NotificationType.DonationApproved,
                    Title = "تم قبول التبرع",
                    Message = $"تمت الموافقة على التبرع رقم {donationOrder.Id}.",
                    EntityType = nameof(DonationOrder),
                    EntityId = donationOrder.Id
                });
            }

            return "تم الموافقة على التبرع بنجاح";
        }
        //--------------------------------------------------------------------------------------
        public async Task<string> RejectDonation(int DonationOrderId, string RejectReason)
        {
            // جيب الـ DonationOrder
            var donationOrder = await _unitOfWork.Repository<DonationOrder, int>()
                .GetQueryable()
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(d => d.Id == DonationOrderId);

            if (donationOrder == null)
                return "التبرع مش موجود";


            // جيب الـ OrderItems المرتبطة بالـ DonationOrder ده
            var orderItems = await _unitOfWork.Repository<OrderItem, int>()
                .GetQueryable()
                .Where(oi => oi.DonationOrderId == DonationOrderId && !oi.IsDeleted)
                .ToListAsync();

            if (orderItems == null || !orderItems.Any())
                return "مفيش items مرتبطة بالتبرع ده";

            if (orderItems.Any(oi => oi.orderItemStatus == RequestedStatus.Approved))
                return "مينفعش ترفض  تبرع اتوافق عليه قبل كده";

            // غير الـ status لـ Rejected وحط سبب الرفض
            foreach (var item in orderItems)
            {
                item.orderItemStatus = RequestedStatus.Rejected;
                item.RejectReason = RejectReason;
            }

            await _unitOfWork.Complete();

            if (!string.IsNullOrWhiteSpace(donationOrder.Donor?.AppUserId))
            {
                await _notificationService.NotifyUserAsync(donationOrder.Donor.AppUserId, new NotificationMessageDto
                {
                    Type = NotificationType.DonationRejected,
                    Title = "تم رفض التبرع",
                    Message = $"تم رفض التبرع رقم {donationOrder.Id}. السبب: {RejectReason}",
                    EntityType = nameof(DonationOrder),
                    EntityId = donationOrder.Id
                });
            }

            return "تم رفض التبرع ";
        }
        //---------------------------------------------------------------------------------------



        #endregion


  
    }
}
