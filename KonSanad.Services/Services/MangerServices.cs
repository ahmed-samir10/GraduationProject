using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KonSanad.Core.Entity;
using KonSanad.Core.Entity.Enums;

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
        public async Task<IEnumerable<GetHelpOrderDto>> GetHelpOrderAsync()
        {

            var query = _unitOfWork.Repository<HelpOrder, int>().GetQueryable();

            var result = await query
                .Include(h => h.Beneficiary)
                .Include(h => h.HelpOrderItems)
                .Where(h => !h.IsDeleted)
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
                .Where(v => !v.IsDeleted && v.Status== Core.Entity.Enums.RequestedStatus.Approved)

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

    }
}
