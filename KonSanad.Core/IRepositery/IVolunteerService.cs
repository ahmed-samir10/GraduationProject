using System.Collections.Generic;
using System.Threading.Tasks;
using KonSanad.Core.Dtos;

namespace KonSanad.Core.IServices
{
    public interface IVolunteerService
    {
        Task<IEnumerable<MissionDto>> GetAllMissionsAsync();
        Task StartMissionAsync(int volunteerId, int missionId);
    }
}