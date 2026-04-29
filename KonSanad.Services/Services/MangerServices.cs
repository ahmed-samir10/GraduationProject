using KonSanad.Core.Dtos.ManagerDtos;
using KonSanad.Core.Entities;
using KonSanad.Core.Entity;
using KonSanad.Core.Entity.Enums;
using KonSanad.Repository.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        //------------------------------------------------------------------------------------------------------


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
                var order = await query.FirstOrDefaultAsync(h => h.Id == helpOrderId.Value);

                if (order == null)
                    return Enumerable.Empty<GetHelpOrderDto>();

                var itemsQuery = order.HelpOrderItems.AsQueryable();

                // ✅ فلترة بالـ status على الـ HelpOrderItem بس
                if (status.HasValue)
                    itemsQuery = itemsQuery.Where(i => i.HelpOrderStatus == status.Value);

                // ✅ فلترة بالـ search على اسم المستفيد لو موجود
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var beneficiaryName = order.Beneficiary?.FullName ?? string.Empty;
                    if (!beneficiaryName.Contains(search, StringComparison.OrdinalIgnoreCase))
                        return Enumerable.Empty<GetHelpOrderDto>();
                }

                // ✅ لو مفيش items بعد الفلترة رجع empty دايماً
                // لأن الفيلتر بيكون على الـ HelpOrderItem بس مش على الـ HelpOrder
                if (!itemsQuery.Any())
                    return Enumerable.Empty<GetHelpOrderDto>();

                return MapItemsToDto(order, itemsQuery);
            }

            // ✅ search عام على اسم المستفيد أو التليفون أو العنوان أو عنوان الـ item
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(h =>
                    h.Beneficiary.FullName.Contains(search) ||
                    h.Beneficiary.Phone.Contains(search));

            // ✅ filter بالـ status - جيب الـ orders اللي فيها items بالـ status ده
            if (status.HasValue)
                query = query.Where(h =>
                    h.HelpOrderItems.Any(i => i.HelpOrderStatus == status.Value));

            // ✅ pagination
            var orders = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ✅ mapping
            return orders.SelectMany(h =>
            {
                var items = h.HelpOrderItems?.AsQueryable();

                // ✅ فلتر الـ items بالـ status لو موجود عشان مترجعش items تانية
                if (status.HasValue && items != null)
                    items = items.Where(i => i.HelpOrderStatus == status.Value);

                return items != null && items.Any()
                    ? MapItemsToDto(h, items)
                    : MapOrderToDto(h);
            });
        }

        // ✅ Helper - map الـ order من غير items
        private IEnumerable<GetHelpOrderDto> MapOrderToDto(HelpOrder order) =>
            new List<GetHelpOrderDto>
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

        // ✅ Helper - map الـ items بتاعت الـ order
        private IEnumerable<GetHelpOrderDto> MapItemsToDto(HelpOrder order, IQueryable<HelpOrderItem> items) =>
            items.Select(i => new GetHelpOrderDto
            {
                Id = order.Id,
                ItemId = i.Id,
                UserName = order.Beneficiary.FullName ?? string.Empty,
                PhoneNumber = order.Beneficiary.Phone ?? string.Empty,
                Address = order.Beneficiary.Address ?? string.Empty,
                HelpOrderStatus = i.HelpOrderStatus,
                CreatedAt = order.CreatedAt,
                Tilte = i.Title,
                Reason = i.reason,
                Quantity = i.QuantityRequested
            });

        //------------------------------------------------------------------------------------------------------
        // Get Volunteer in Charity

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

        public async Task<string> RejectItem(int missionId, int itemId)
        {
            var mission = await _unitOfWork.Repository<Mission, int>()
                .GetQueryable()
                .Include(m => m.HelpOrder)
                    .ThenInclude(h => h.HelpOrderItems)
                .FirstOrDefaultAsync(m => m.Id == missionId);

            if (mission == null)
                return "المهمة غير موجودة";

            var item = mission.HelpOrder.HelpOrderItems
                .FirstOrDefault(i => i.Id == itemId);

            if (item == null)
                return "العنصر غير موجود";

            // ❌ Update item
            item.Status = RequestedStatus.Rejected;
            item.HelpOrderStatus = RequestedStatus.Rejected;
            item.UpdatedAt = DateTime.UtcNow;

            await UpdateOrderStatus(mission.HelpOrder);

            await _unitOfWork.Complete();

            return "تم رفض العنصر";
        }
        public async Task<string> AcceptItem(int missionId, int itemId)
        {
            var mission = await _unitOfWork.Repository<Mission, int>()
                .GetQueryable()
                .Include(m => m.HelpOrder)
                    .ThenInclude(h => h.HelpOrderItems)
                .FirstOrDefaultAsync(m => m.Id == missionId);

            if (mission == null)
                return "المهمة غير موجودة";

            var item = mission.HelpOrder.HelpOrderItems
                .FirstOrDefault(i => i.Id == itemId);

            if (item == null)
                return "العنصر غير موجود";

            var supply = await _unitOfWork.Repository<Supply, int>()
                .FirstOrDefaultAsync(s => s.Id == item.SupplyId);

            if (supply == null)
                return "العنصر غير موجود في المخزن";



            // ✅ Update item
            item.Status = RequestedStatus.Approved;
            item.HelpOrderStatus = RequestedStatus.Approved;
            item.UpdatedAt = DateTime.UtcNow;

            // ✅ Update stock
            supply.StockQuantity -= item.QuantityRequested;

            // ✅ Update order
            await UpdateOrderStatus(mission.HelpOrder);

            await _unitOfWork.Complete();

            return "تم قبول العنصر";
        }
        private async Task UpdateOrderStatus(HelpOrder order)
        {
            var items = order.HelpOrderItems;

            if (items.All(i => i.Status == RequestedStatus.Approved))
                order.Status = RequestedStatus.Approved;

            else if (items.All(i => i.Status == RequestedStatus.Rejected))
                order.Status = RequestedStatus.Rejected;

            else
                order.Status = RequestedStatus.Pending;

            order.UpdatedAt = DateTime.UtcNow;
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
        //-----------------------------------------------------------------------------------------------

        // Get All Volunteer Requests [ Join a campaigns + Start Mission ]
        public async Task<IEnumerable<GetVolunteerRequestsDto>> GetAllVolunteerRequestsAsync(
      int pageNumber,
      int pageSize,
      string? search = null,
      RefType? requestType = null,
      VolunteerMissionStatus? missionStatus = null,
      RequestedStatus? campaignStatus = null)
        {
            var result = new List<GetVolunteerRequestsDto>();

            // ============================================================
            // 🛑 Conflict Validation
            // لو requestType == Campaign وجاب missionStatus → return empty
            // لو requestType == Mission وجاب campaignStatus → return empty
            // ============================================================
            if (requestType == RefType.Campaign && missionStatus.HasValue)
                return result; // empty

            if (requestType == RefType.Mission && campaignStatus.HasValue)
                return result; // empty

            // ============================================================
            // 1️⃣ Campaign Requests
            // ============================================================
            if (requestType == null || requestType == RefType.Campaign)
            {
                IQueryable<CampaignVolunteer> campaignQuery = _unitOfWork.Repository<CampaignVolunteer, int>()
                    .GetQueryable()
                    .Where(cv => !cv.IsDeleted &&
                        (cv.Status == RequestedStatus.Pending ||
                         cv.Status == RequestedStatus.Rejected ||
                         cv.Status == RequestedStatus.Approved))
                    .Include(cv => cv.Volunteer)
                    .Include(cv => cv.Campaign);

                if (requestType == RefType.Campaign && campaignStatus.HasValue)
                    campaignQuery = campaignQuery.Where(cv => cv.Status == campaignStatus.Value);

                if (requestType == RefType.Campaign && !string.IsNullOrWhiteSpace(search))
                    campaignQuery = campaignQuery.Where(cv =>
                        cv.Volunteer.FullName.Contains(search) ||
                        cv.Volunteer.Phone.Contains(search));

                var campaignRequests = await campaignQuery
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
            // ============================================================
            if (requestType == null || requestType == RefType.Mission)
            {
                IQueryable<MissionVolunteers> missionQuery = _unitOfWork.Repository<MissionVolunteers, int>()
                    .GetQueryable()
                    .Where(mv => !mv.IsDeleted &&
                        (mv.MissionStatus == VolunteerMissionStatus.InReview ||
                         mv.MissionStatus == VolunteerMissionStatus.Rejected ||
                         mv.MissionStatus == VolunteerMissionStatus.WaitingForReport ||
                         mv.Mission.MissionStatus == MissionStatus.Assigend))
                    .Include(mv => mv.Volunteer)
                    .Include(mv => mv.Mission);

                if (requestType == RefType.Mission && missionStatus.HasValue)
                    missionQuery = missionQuery.Where(mv => mv.MissionStatus == missionStatus.Value);

                if (requestType == RefType.Mission && !string.IsNullOrWhiteSpace(search))
                    missionQuery = missionQuery.Where(mv =>
                        mv.Volunteer.FullName.Contains(search));

                var missionRequests = await missionQuery
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
            // 3️⃣ السيرش العام
            // ============================================================
            if (requestType == null && !string.IsNullOrWhiteSpace(search))
                result = result.Where(r =>
                    r.VolunteerName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    r.Phone.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    r.Address.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    r.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();

            // ============================================================
            // 4️⃣ Pagination + Sorting
            // ============================================================
            result = result
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return result;
        }
        //------------------------------------------------------------------------------------------------
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
            // ============================================================
            if (category == null || category == DonationCategory.Cash)
            {
                var cashDonations = await _unitOfWork.Repository<CashDonation, int>()
                    .GetQueryable()
                    .Include(cd => cd.DonationOrder)
                        .ThenInclude(d => d.Donor)
                    .Where(cd => !cd.IsDeleted)
                    .Where(cd => status == null || (int)cd.cashDonationStatus == (int)status)
                    .Select(cd => new GetAllDonationDto
                    {
                        Id = cd.DonationOrder.Id,
                        ItemId = cd.Id,   // ✅ Id بتاع الـ CashDonation نفسه
                        UserName = cd.DonationOrder.Donor.FullName,
                        DonationStatus = (RequestedStatus)cd.cashDonationStatus,
                        DonationCategory = DonationCategory.Cash,
                        CreatedAt = cd.CreatedAt,
                        AmountCashDonation = cd.Amount,
                        CampaignTitle = cd.CampaignId != 0
                            ? _unitOfWork.Repository<Campaign, int>()
                                .GetQueryable()
                                .Where(c => c.Id == cd.CampaignId)
                                .Select(c => c.Title)
                                .FirstOrDefault()
                            : null,
                        SupplyName = cd.CampaignId == 0 && cd.HelpOrderId != 0
                            ? _unitOfWork.Repository<Supply, int>()
                                .GetQueryable()
                                .Where(s => s.Id == cd.HelpOrderId)
                                .Select(s => s.Name)
                                .FirstOrDefault()
                            : null,
                        Quantity = 0
                    })
                    .ToListAsync();

                result.AddRange(cashDonations);
            }

            // ============================================================
            // 2️⃣ Item Donations
            // ============================================================
            if (category == null || category == DonationCategory.Item)
            {
                var itemDonations = await _unitOfWork.Repository<OrderItem, int>()
                    .GetQueryable()
                    .Include(oi => oi.DonationOrder)
                        .ThenInclude(d => d.Donor)
                    .Where(oi => !oi.IsDeleted)
                    .Where(oi => status == null || (int)oi.orderItemStatus == (int)status)
                    .Select(oi => new GetAllDonationDto
                    {
                        Id = oi.DonationOrder.Id,
                        ItemId = oi.Id,   // ✅ Id بتاع الـ OrderItem من جدول OrderItems
                        UserName = oi.DonationOrder.Donor.FullName,
                        DonationStatus = oi.orderItemStatus != null
                            ? (RequestedStatus)oi.orderItemStatus
                            : RequestedStatus.Pending,
                        DonationCategory = DonationCategory.Item,
                        CreatedAt = oi.CreatedAt,
                        SupplyId = oi.SupplyId.HasValue ? oi.SupplyId.Value : 0,
                        SupplyName = oi.CustomSupplyName ?? "",
                        Quantity = oi.Quantity,
                        AmountCashDonation = 0,
                        CampaignTitle = null
                    })
                    .ToListAsync();

                result.AddRange(itemDonations);
            }

            // ============================================================
            // 3️⃣ Search
            // ============================================================
            if (!string.IsNullOrWhiteSpace(search))
                result = result.Where(r =>
                    r.UserName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    r.SupplyName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (r.CampaignTitle != null && r.CampaignTitle.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    r.AmountCashDonation.ToString().Contains(search)
                ).ToList();

            // ============================================================
            // 4️⃣ Sorting + Pagination
            // ============================================================
            return result
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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

        public async Task<string> AcceptDonationItem(int donationOrderId, int itemId)
        {
            var donationOrder = await _unitOfWork.Repository<DonationOrder, int>()
                .GetQueryable()
                .Include(d => d.Donor)
                .FirstOrDefaultAsync(d => d.Id == donationOrderId);

            if (donationOrder == null)
                return "التبرع مش موجود";

            var item = await _unitOfWork.Repository<OrderItem, int>()
                .GetQueryable()
                .FirstOrDefaultAsync(i => i.Id == itemId && i.DonationOrderId == donationOrderId && !i.IsDeleted);

            if (item == null)
                return "العنصر غير موجود";

            if (item.orderItemStatus != RequestedStatus.Pending)
                return "العنصر تم التعامل معه قبل كده";

            // ✅ Accept
            item.orderItemStatus = RequestedStatus.Approved;

            if (donationOrder.DonationCategoryStatus == "custom")
            {
                var supply = new Supply
                {
                    Name = item.CustomSupplyName,
                    Description = item.Description,
                    Unit = item.Unit,
                    StockQuantity = item.Quantity,
                    MinimumLevel = 5,
                    Category = "اخري",
                    CategoryId = 16,
                    Status = RequestedStatus.Approved,
                    IsDeleted = false,
                    ImageUrl = item.ImageUrl,
                    Title = "غذائي",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };

                await _unitOfWork.Repository<Supply, int>().AddAsync(supply);
            }
            else if (donationOrder.DonationCategoryStatus == "donationItem")
            {
                var supply = await _unitOfWork.Repository<Supply, int>()
                    .FirstOrDefaultAsync(s => s.Id == item.SupplyId);

                if (supply == null)
                    return "العنصر مش موجود في المخزن";

                supply.StockQuantity += item.Quantity;
                supply.UpdatedAt = DateTime.Now;
            }

            await UpdateDonationOrderStatus(donationOrderId);

            await _unitOfWork.Complete();

            return "تم قبول العنصر";
        }
        #endregion

        public async Task<string> RejectDonationItem(int donationOrderId, int itemId, string reason)
        {
            var item = await _unitOfWork.Repository<OrderItem, int>()
                .GetQueryable()
                .FirstOrDefaultAsync(i => i.Id == itemId && i.DonationOrderId == donationOrderId && !i.IsDeleted);

            if (item == null)
                return "العنصر غير موجود";

            if (item.orderItemStatus != RequestedStatus.Pending)
                return "العنصر تم التعامل معه قبل كده";

            // ❌ Reject
            item.orderItemStatus = RequestedStatus.Rejected;
            item.RejectReason = reason;

            await UpdateDonationOrderStatus(donationOrderId);

            await _unitOfWork.Complete();

            return "تم رفض العنصر";
        }
        private async Task UpdateDonationOrderStatus(int donationOrderId)
        {
            var items = await _unitOfWork.Repository<OrderItem, int>()
                .GetQueryable()
                .Where(i => i.DonationOrderId == donationOrderId && !i.IsDeleted)
                .ToListAsync();

            var donationOrder = await _unitOfWork.Repository<DonationOrder, int>()
                .FirstOrDefaultAsync(d => d.Id == donationOrderId);

            if (items.All(i => i.orderItemStatus == RequestedStatus.Approved))
                donationOrder.Status = RequestedStatus.Approved;

            else if (items.All(i => i.orderItemStatus == RequestedStatus.Rejected))
                donationOrder.Status = RequestedStatus.Rejected;

            else
                donationOrder.Status = RequestedStatus.Pending;

            donationOrder.UpdatedAt = DateTime.Now;
        }
        #region المستودع

        public async Task<IEnumerable<GetSupplyDto>> GetSuppliesAsync(
     int pageNumber,
     int pageSize,
     string? search = null,
     SupplyCategory? category = null) // ✅ أضف ده
        {
            var query = _unitOfWork.Repository<Supply, int>()
                .GetQueryable()
                .Where(s => !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s =>
                    s.Name.Contains(search) ||
                    s.StockQuantity.ToString().Contains(search) ||
                    s.MinimumLevel.ToString().Contains(search));

            // ✅ أضف الـ filter ده
            if (category.HasValue)
                query = query.Where(s => s.CategoryId == (int)category.Value);

            var result = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new GetSupplyDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Unit = s.Unit,
                    StockQuantity = s.StockQuantity,
                    MinimumLevel = s.MinimumLevel,
                    ImageUrl = s.ImageUrl,
                    supplyCategory = (SupplyCategory)s.CategoryId,
                    UpdatedAt = s.UpdatedAt,
                    IsLowStock = s.StockQuantity < s.MinimumLevel
                })
                .ToListAsync();

            return result;
        }
        //-----------------------------------------------------------------------------------
        public async Task<string> UpdateSupplyAsync(int supplyId, UpdateSupplyDto dto)
        {
            var supply = await _unitOfWork.Repository<Supply, int>()
                .GetQueryable()
                .FirstOrDefaultAsync(s => s.Id == supplyId && !s.IsDeleted);

            if (supply == null)
                return "الصنف غير موجود";


            if (dto.StockQuantity.HasValue && dto.StockQuantity.Value > 0)
                supply.StockQuantity = dto.StockQuantity.Value;

            if (dto.MinimumLevel.HasValue && dto.MinimumLevel.Value > 0)
                supply.MinimumLevel = dto.MinimumLevel.Value;



            supply.UpdatedAt = DateTime.Now;

            _unitOfWork.Repository<Supply, int>().Update(supply);
            await _unitOfWork.Complete();

            if (supply.StockQuantity < supply.MinimumLevel)
            {
                await _notificationService.NotifyManagersAsync(new NotificationMessageDto
                {
                    Title = "تنبيه مخزون منخفض",
                    Message = $"كمية الصنف {supply.Name} أقل من الحد الأدنى.",
                    EntityType = nameof(Supply),
                    EntityId = supply.Id
                });
            }

            return "تم تعديل الصنف بنجاح";
        }
        //---------------------------------------------------------------------------------
        public async Task<string> ControlIsVisibility(ControlSupplyDto controlSupply)
        {
            var supply = await _unitOfWork.Repository<Supply, int>().GetByIdAsync(controlSupply.SupplyId);
            if (supply == null) return "لا يوجد منتج";
            supply.ShowinType = controlSupply.ShowIn;
            supply.UpdatedAt = DateTime.Now;
            await _unitOfWork.Complete();
            return " نم الاضافه بنجاح";
        }
        public async Task<byte[]> GenerateLowStockReportAsync()
        {
            var Products = await _unitOfWork.Repository<Supply, int>()
                .GetQueryable()
                .Where(s => s.StockQuantity < s.MinimumLevel)
                .ToListAsync();
            return GeneratePdf(Products);



            #endregion
        }
        private byte[] GeneratePdf(List<Supply> supplies)
        {
            return QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Header()
                        .Text("Low Stock Report")
                        .FontSize(20)
                        .Bold();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Name").Bold();
                            header.Cell().Text("Quantity").Bold();
                            header.Cell().Text("Min Quantity").Bold();
                        });

                        foreach (var supply in supplies)
                        {
                            table.Cell().Text(supply.Name);
                            table.Cell().Text(supply.StockQuantity.ToString());
                            table.Cell().Text(supply.MinimumLevel.ToString());
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated At: {DateTime.Now}");
                });
            }).GeneratePdf();
        }
    }
}
