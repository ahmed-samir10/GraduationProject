using KonSanad.Core.Dtos;
using KonSanad.Core.Entity;
using KonSanad.Core.IRepositery;

using KonSanad.Repository.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KonSanad.Services.Services
{
    public class VolunteerService : IVolunteerService
    {
        private readonly KonSanadDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public VolunteerService(KonSanadDbContext context , IUnitOfWork unitOfWork , IFileService fileService)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }
        public async Task<IEnumerable<MissionDto>> GetAllMissionsAsync()
        {
            var dtos = await _context.Missions
                .AsNoTracking()
                .Select(m => new MissionDto
                {
                    Id = m.Id,

                    Title = m.HelpOrder != null && m.HelpOrder.Beneficiary != null
                        ? m.HelpOrder.Beneficiary.FullName
                        : (m.Title ?? string.Empty),

                    PhoneNumber = m.HelpOrder != null && m.HelpOrder.Beneficiary != null
                        ? m.HelpOrder.Beneficiary.Phone
                        : (m.PhoneNumber ?? string.Empty),

                    Address = m.HelpOrder != null && m.HelpOrder.Beneficiary != null
                        ? m.HelpOrder.Beneficiary.Address
                        : (m.Address ?? string.Empty),

                    Description = "مطلوب زياره ميدانيه",
                    StartDate = m.CreatedAt,
                    MissionStatus = m.MissionStatus
                })
                .ToListAsync();

            return dtos;
        }

        public async Task StartMissionAsync(int volunteerId, int missionId)
        {
            /*   var existing = await _unitOfWork
                .Repository<VolunteerMission, int>()
                .FindAllAsync(x => x.MissionId == missionId);

               if (existing != null)
                   throw new Exception("Mission already started");*/

            var MissionVolunteer = new MissionVolunteers
            {
                VolunteerId = volunteerId,
                MissionId = missionId,
                CreatedAt = DateTime.UtcNow,
                MissionStatus = MissionStatus.InProgress
            };

            await _unitOfWork.Repository<MissionVolunteers, int>()
                .AddAsync(MissionVolunteer);

            await _unitOfWork.Complete();
        }


        // ----------------------------------------------------------
        // Method to submit a mission report by a volunteer
        // ----------------------------------------------------------
        public async Task<string> SubmitMissionReportAsync(int volunteerId, int missionId, IFormFile file, string? notes = null)
        {
            // Validate volunteer exists
            var volunteer = await _unitOfWork.Repository<Volunteer, int>().FirstOrDefaultAsync(v => v.Id == volunteerId);

            if (volunteer is null) throw new ArgumentException("Volunteer not found");



            // Validate volunteer-mission relation exists Or not
            var vmRepo = _unitOfWork.Repository<MissionVolunteers, int>();
            var volunteerMission = await vmRepo.FirstAsync(vm => vm.VolunteerId == volunteerId && vm.MissionId == missionId);

            if (volunteerMission is null)
                throw new ArgumentException("VolunteerMission not found for given volunteer and mission");



            // Optionally validate mission exists and dates (if needed)
            var mission = await _unitOfWork.Repository<Mission, int>().FirstAsync(m => m.Id == missionId);
            if (mission is null)
                throw new ArgumentException("Mission not found");



            // If mission has start/end dates, you can prevent reporting outside mission window
            if (mission.StartDate.HasValue && mission.EndDate.HasValue)
            {
                var now = DateTime.UtcNow;
                if (now < mission.StartDate.Value || now > mission.EndDate.Value)
                {
                    throw new ArgumentException("Cannot submit report outside mission active period");
                }
            }


            // Save file
            var relativeUrl = await _fileService.SaveFileAsync(file, "uploads");


            // Store the result (for now store a single URL).
            // If multiple files are needed consider storing JSON or separate table.
            volunteerMission.ResultMission = relativeUrl;
            if (!string.IsNullOrEmpty(notes))
            {
                volunteerMission.Notes = notes;
            }

            vmRepo.Update(volunteerMission);
            volunteerMission.MissionStatus = MissionStatus.Completed;
            volunteerMission.Status ="Completed";
            await _unitOfWork.Complete();

            return relativeUrl;
        }
        // ----------------------------------------------------------
        // ----------------------------------------------------------

    }
}