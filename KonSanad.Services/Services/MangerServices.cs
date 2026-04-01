using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Entities;
using KonSanad.Core.Entity;
using KonSanad.Core.Entity.Enums;
using Microsoft.EntityFrameworkCore;

namespace KonSanad.Services.Services
{
    public class MangerServices : IMangerServices
    {
        private readonly IUnitOfWork _unitOfWork;
        public MangerServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }

        public async Task<string> AssignVolunteerAsync(int helpOrderId, int volunteerId)
        {
            // 1️⃣ هات الـ HelpOrder
            var helpOrder = await _unitOfWork.Repository<HelpOrder, int>()
      .GetQueryable()
      .Include(h => h.Beneficiary)  //غيرت ال status ال فجدول المستفيدين فالداتا بيز ل int <== xxx
      .FirstOrDefaultAsync(h => h.Id == helpOrderId);

            if (helpOrder == null)
                return "HelpOrder not found";

            // 2️⃣ Create Mission
            var mission = new Mission
            {
                Title = helpOrder.Beneficiary.FullName,
                Description = "زيارة ميدانية لمتابعة أحوال الأسر المحتاجة",
                Address = helpOrder.Beneficiary.Address,
                PhoneNumber = helpOrder.Beneficiary.Phone,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(3), // حسب اللوجيك بتاعك
                ManagerId = 1,
                HelpOrderId = helpOrder.Id,

                // CampaignId = helpOrder.CampaignId,

                MissionStatus = Core.Entity.Enums.MissionStatus.Assigend, // Pending مثلا

                // status في الداتا بيز not null لازم يكون ليه قيمة
                Status = RequestedStatus.Pending,

                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Mission, int>().AddAsync(mission);
            await _unitOfWork.Complete(); // مهم عشان تاخد Id

            // 3️⃣ Create MissionVolunteer
            var missionVolunteer = new MissionVolunteers
            {
                MissionId = mission.Id,
                VolunteerId = volunteerId,
                MissionStatus = Core.Entity.Enums.VolunteerMissionStatus.WaitingForReport, // WaitReport

                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<MissionVolunteers, int>().AddAsync(missionVolunteer);

            await _unitOfWork.Complete();

            return "Volunteer assigned successfully";
        }

        //Get HelpOrder 
        public async Task<IEnumerable<GetHelpOrderDto>> GetHelpOrderAsync(int pageNumber, int pageSize)
        {

            var query = _unitOfWork.Repository<HelpOrder, int>().GetQueryable();

            var result = await query
                .Include(h => h.Beneficiary)
                .Include(h => h.HelpOrderItems)
                .Where(h => !h.IsDeleted)
                 .Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize)
                .Select(h => new GetHelpOrderDto
                {
                    Id = h.Id,
                    UserName = h.Beneficiary.FullName,
                    Tilte = h.HelpOrderItems
                                .Select(i => i.Title)
                                .FirstOrDefault(),
                    PhoneNumber = h.Beneficiary.Phone,
                    Address = h.Beneficiary.Address,
                    HelpOrderStatus = h.HelpOrderStatus,
                    CreatedAt = h.CreatedAt,

                    Reason = h.HelpOrderItems
                                .Select(i => i.reason)
                                .FirstOrDefault(),

                    Quantity = h.HelpOrderItems
                                .Select(i => i.QuantityRequested)
                                .FirstOrDefault()
                })
                .ToListAsync();

            return result;
        }

        // Get Volunteer in Charity
        public async Task<IEnumerable<GetVolunteerDto>> GetVolunteerAsync()
        {
            var query = _unitOfWork.Repository<Volunteer, int>().GetQueryable();

            var result = await query
                .Where(v => !v.IsDeleted && v.Status == Core.Entity.Enums.RequestedStatus.Approved)

                .Select(v => new GetVolunteerDto
                {
                    Id = v.Id,
                    Name = v.FullName,

                    Location = v.Address,

                    // لو Skills List أو Navigation Property
                    //   Skills = string.Join(", ", v.Skills.Select(s => s.)),
                    Skills = v.Skills,

                    //
                    Availability = v.Availability.ToString()
                })
                .ToListAsync();

            return result;
        }

        public async Task<string> RejectOrder(int missionId)
        {
            var mission = await _unitOfWork.Repository<Mission, int>()
               .GetQueryable()
               .Include(m => m.HelpOrder)
                   .ThenInclude(h => h.HelpOrderItems)
               .FirstOrDefaultAsync(m => m.Id == missionId);

            if (mission == null)
                return "Mission not found";

            var vm = await _unitOfWork.Repository<MissionVolunteers, int>()
                .GetQueryable()
                .FirstOrDefaultAsync(mv => mv.MissionId == missionId);

            if (vm == null)
                return "Volunteer mission not found";

            // ✅ Update mission
            mission.MissionStatus = MissionStatus.Completed;
            mission.HelpOrder.HelpOrderItems.FirstOrDefault().HelpOrderStatus = RequestedStatus.Rejected; // لو عايز ترفض أول item
            mission.HelpOrder.HelpOrderItems.FirstOrDefault().Status = RequestedStatus.Rejected; // لو عندك حقل Status في HelpOrderItem نفسه
            mission.HelpOrder.HelpOrderItems.FirstOrDefault().UpdatedAt = DateTime.Now; // لو عايز تحدث وقت التحديث
            mission.HelpOrder.ManagerId = 1; // Assign to manager with ID 1 (or any logic you want)
            mission.HelpOrder.Status = RequestedStatus.Rejected; // لو عندك حقل Status في HelpOrder نفسه
            mission.HelpOrder.UpdatedAt = DateTime.Now;
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

            return "تم رفض الطلب ";

        }
        public async Task<string> AcceptOrder(int missionId)
        {
            var mission = await _unitOfWork.Repository<Mission, int>()
                .GetQueryable()
                .Include(m => m.HelpOrder)
                    .ThenInclude(h => h.HelpOrderItems)
                .FirstOrDefaultAsync(m => m.Id == missionId);

            if (mission == null)
                return "Mission not found";

            var vm = await _unitOfWork.Repository<MissionVolunteers, int>()
                .GetQueryable()
                .FirstOrDefaultAsync(mv => mv.MissionId == missionId);

            if (vm == null)
                return "Volunteer mission not found";

            // ✅ Update mission
            mission.MissionStatus = MissionStatus.Completed;


            // ✅ Update volunteer mission
            vm.MissionStatus = VolunteerMissionStatus.Completed;

            // ✅ Update help order
            mission.HelpOrder.HelpOrderItems.FirstOrDefault().HelpOrderStatus = RequestedStatus.Approved; // لو عايز توافق على أول item
            mission.HelpOrder.HelpOrderItems.FirstOrDefault().Status = RequestedStatus.Approved; // لو عندك حقل Status في HelpOrderItem نفسه
            mission.HelpOrder.HelpOrderItems.FirstOrDefault().UpdatedAt = DateTime.Now; // لو عايز تحدث وقت التحديث
            mission.HelpOrder.HelpOrderStatus = RequestedStatus.Approved;
            mission.HelpOrder.ManagerId = 1; // Assign to manager with ID 1 (or any logic you want)
            mission.HelpOrder.Status = RequestedStatus.Approved; // لو عندك حقل Status في HelpOrder نفسه
            mission.HelpOrder.UpdatedAt = DateTime.Now; // لو عايز تحدث وقت التحديث  

            // ✅ Update first item (لو موجود)
            var item = mission.HelpOrder.HelpOrderItems.FirstOrDefault();
            if (item != null)
                item.HelpOrderStatus = RequestedStatus.Approved;

            // ✅ Save changes
            await _unitOfWork.Complete();

            return "تم قبول الطلب بنجاح";
        }
        public async Task<ViewReportDto> ViewReport(int helpOrderId)
        {
            var mission = await _unitOfWork.Repository<Mission, int>()
                .GetQueryable()
                .Include(m => m.HelpOrder)
                    .ThenInclude(h => h.Beneficiary)
                .Include(m => m.HelpOrder)
                    .ThenInclude(h => h.HelpOrderItems)
                .FirstOrDefaultAsync(m => m.HelpOrderId == helpOrderId);

            if (mission == null)
                return null;

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

        public async Task<IEnumerable<GetJoinVolunteerDto>> GetJoinVolunteer(int pageNumber, int pageSize)
        {
            var volunteers = await _unitOfWork.Repository<Volunteer, int>()
                .GetQueryable()
                .Where(v => v.Status == RequestedStatus.Pending && !v.IsDeleted)
                 .Skip((pageNumber - 1) * pageSize)
                 .Take(pageSize)// pending + مش محذوف
                .Select(v => new GetJoinVolunteerDto
                {
                    Id = v.Id,
                    Status = RequestedStatus.Pending,
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
                return "Volunteer not found";
            volunteer.Status = RequestedStatus.Approved;
            volunteer.ManagerId = 1; // Assign to manager with ID 1 (or any logic you want)
            _unitOfWork.Repository<Volunteer, int>().Update(volunteer);
            await _unitOfWork.Complete();
            return "Volunteer accepted successfully";
        }

        public async Task<string> RejectVolunteer(int VolId)
        {
            var volunteer = _unitOfWork.Repository<Volunteer, int>()
                 .GetQueryable()
                 .FirstOrDefault(v => v.Id == VolId);
            if (volunteer == null)
                return "Volunteer not found";
            volunteer.Status = RequestedStatus.Rejected;
            volunteer.ManagerId = 1; // Assign to manager with ID 1 (or any logic you want)
            _unitOfWork.Repository<Volunteer, int>().Update(volunteer);
            await _unitOfWork.Complete();
            return "Volunteer rejected successfully";

        }
        // Get All Volunteer Requests [ Join a campaigns + Start Mission ]
        public async Task<IEnumerable<GetVolunteerRequestsDto>> GetAllVolunteerRequestsAsync(int pageNumber, int pageSize)
        {
            var result = new List<GetVolunteerRequestsDto>();

            // 1️⃣ Campaign Join Requests
            var campaignRequests = await _unitOfWork.Repository<CampaignVolunteer, int>()
                .GetQueryable()
                .Where(cv => !cv.IsDeleted && cv.Status == RequestedStatus.Pending)
                .Include(cv => cv.Volunteer)
                .Include(cv => cv.Campaign)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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

            // 2️⃣ Mission Start Requests
            var missionRequests = await _unitOfWork.Repository<MissionVolunteers, int>()
                .GetQueryable()
                .Where(mv => !mv.IsDeleted && mv.MissionStatus == VolunteerMissionStatus.InReview)
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

            result.AddRange(campaignRequests);
            result.AddRange(missionRequests);

            // 3️⃣ Sort by newest first
            return result.OrderByDescending(r => r.CreatedAt);
        }

        public async Task<string> AcceptRequst(int requstType, int referenceId)
        {
            // 🔹 Campaign Request
            if (requstType == (int)RefType.Campaign)
            {
                var campaignRequest = await _unitOfWork.Repository<CampaignVolunteer, int>()
                    .GetQueryable()
                    .FirstOrDefaultAsync(c => c.Id == referenceId);

                if (campaignRequest == null)
                    return "Campaign request not found";

                campaignRequest.Status = RequestedStatus.Approved;

                await _unitOfWork.Complete();

                return "Campaign request accepted successfully";
            }

            // 🔹 Mission Request
            else if (requstType == (int)RefType.Mission)
            {
                var missionRequest = await _unitOfWork.Repository<MissionVolunteers, int>()
                    .GetQueryable()
                    .Include(mv => mv.Mission)
                        .ThenInclude(m => m.HelpOrder)
                    .FirstOrDefaultAsync(mv => mv.Id == referenceId);

                if (missionRequest == null)
                    return "Mission request not found";

                // ✅ Update volunteer mission
                missionRequest.MissionStatus = VolunteerMissionStatus.WaitingForReport;

                // ✅ ممكن كمان تحدّث المهمة نفسها
                if (missionRequest.Mission != null)
                {
                    missionRequest.Mission.MissionStatus = MissionStatus.Assigend;
                }

                await _unitOfWork.Complete();

                return "Mission request accepted successfully";
            }

            return "Invalid request type";
        }

        public async Task<string> RejectRequst(int requstType, int referenceId)
        {
            // 🔹 Campaign Request
            if (requstType == (int)RefType.Campaign)
            {
                var campaignRequest = await _unitOfWork.Repository<CampaignVolunteer, int>()
                    .GetQueryable()
                    .FirstOrDefaultAsync(c => c.Id == referenceId);

                if (campaignRequest == null)
                    return "Campaign request not found";

                // ✅ منع التكرار
                if (campaignRequest.Status != RequestedStatus.Pending)
                    return "Request already processed";

                campaignRequest.Status = RequestedStatus.Rejected;

                await _unitOfWork.Complete();

                return "Campaign request rejected successfully";
            }

            // 🔹 Mission Request
            else if (requstType == (int)RefType.Mission)
            {
                var missionRequest = await _unitOfWork.Repository<MissionVolunteers, int>()
                    .GetQueryable()
                    .Include(mv => mv.Mission)
                    .FirstOrDefaultAsync(mv => mv.Id == referenceId);

                if (missionRequest == null)
                    return "Mission request not found";

                // ✅ منع التكرار
                if (missionRequest.MissionStatus != VolunteerMissionStatus.InReview)
                    return "Request already processed";

                // ✅ تحديث حالة المتطوع
                missionRequest.MissionStatus = VolunteerMissionStatus.Rejected;

                // (اختياري) ترجع المهمة Pending أو تسيبها زي ما هي
                if (missionRequest.Mission != null)
                {
                    missionRequest.Mission.MissionStatus = MissionStatus.New;
                }

                await _unitOfWork.Complete();

                return "Mission request rejected successfully";
            }

            return "Invalid request type";
        }

        #region Donation
        public async Task<IEnumerable<GetAllDonationDto>> GetDonationAsync(int pageNumber, int pageSize)
        {


            // Cash Donations
            //بجيب كل التبرعات من جدول ال cash donation وربط بجدول DonationOrder و Donor علشان هجيب داتا منهم
            var cashDonations = await _unitOfWork.Repository<CashDonation, int>()
                .GetQueryable()
                .Include(cd => cd.DonationOrder)
                .ThenInclude(d => d.Donor)
                .Where(cd => !cd.IsDeleted) // هات كل التبرعات اللي مش محذوفة.

                .Select(cd => new GetAllDonationDto //بدل ما نرجع Entity من الداتابيز بنحولها إلى DTO.
                {
                    Id = cd.DonationOrder.Id,
                    UserName = cd.DonationOrder.Donor.FullName,

                    DonationStatus = (RequestedStatus)cd.cashDonationStatus,

                    DonationCategory = DonationCategory.Cash,

                    CreatedAt = cd.CreatedAt,

                    AmountCashDonation = cd.Amount,

                    CampaignTitle = cd.CampaignId != 0    // جلب اسم الحملة
                        ? _unitOfWork.Repository<Campaign, int>()  //الوصول لجدول الحملات.
                            .GetQueryable()
                            .Where(c => c.Id == cd.CampaignId)  //اختيار الحملة المطلوبة.
                            .Select(c => c.Title)  // جلب اسم الحملة.
                            .FirstOrDefault()  //إرجاع أول نتيجة.
                        : null,



                    SupplyName = cd.CampaignId == 0 && cd.HelpOrderId != 0  // جلب اسم المنتج  
                        ? _unitOfWork.Repository<Supply, int>()  // الوصول لجدول المنتجات .
                            .GetQueryable()
                            .Where(s => s.Id == cd.HelpOrderId)  // اختيار المنتج المرتبط بالطلب .
                            .Select(s => s.Name) // جلب اسم المنتج.
                            .FirstOrDefault()  //إرجاع أول نتيجة.
                        : null,

                    SupplyId = cd.HelpOrderId.HasValue ? cd.HelpOrderId.Value : 0, // لو فيه HelpOrderId
                                                                                   //  هات قيمته
                                                                                   //  غير كده خليها 0.

                    Quantity = 0  // لأن التبرع النقدي مفيهوش كمية.
                })
                .ToListAsync();


            // Item Donations
            var itemDonations = await _unitOfWork.Repository<OrderItem, int>()
                .GetQueryable()
                .Include(oi => oi.DonationOrder)
                    .ThenInclude(d => d.Donor)
                .Where(oi => !oi.IsDeleted)

                .Select(oi => new GetAllDonationDto
                {
                    Id = oi.DonationOrder.Id,
                    UserName = oi.DonationOrder.Donor.FullName,

                    // ✅ التعديل هنا — الـ Status كان nullable وبيرمي Exception لو NULL
                    DonationStatus = oi.Status != null
                        ? (RequestedStatus)oi.orderItemStatus
                        : RequestedStatus.Pending,

                    DonationCategory = DonationCategory.Item,

                    CreatedAt = oi.CreatedAt,

                    SupplyId = oi.SupplyId ?? 0,
                    SupplyName = oi.CustomSupplyName ?? "",

                    Quantity = oi.Quantity,

                    AmountCashDonation = 0,
                    CampaignTitle = null
                })
                .ToListAsync();

            // Pagination
            var allDonations = cashDonations  //دمج التبرعات    //Cash Donations +  Item Donations                                           
                .Concat(itemDonations)
                .OrderBy(d => d.Id) //ترتيب التبرعات حسب: Id من الأصغر للأكبر
                .ToList();

            return allDonations
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
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
                    DonationStatus = orderItem.Status != null
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
                };

                return dto;
            }

            // ======= Not Found =======
            return null;
        }

        #endregion
    }
}
