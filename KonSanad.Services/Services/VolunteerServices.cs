using KonSanad.Core.Dtos;
using KonSanad.Core.Entity;
using KonSanad.Core.IRepositery;
using KonSanad.Core.IServices;
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

        public VolunteerService(KonSanadDbContext context , IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
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

    }
}