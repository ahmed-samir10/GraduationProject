using KonSanad.Core.Dtos;
using KonSanad.Core.Entity;
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

        public VolunteerService(KonSanadDbContext context)
        {
            _context = context;
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


    }
}