using KonSanad.Core.Dtos;
using Microsoft.AspNetCore.Http;
namespace KonSanad.Core.IRepositery;

public interface IVolunteerService
{
    /// Submit a report file (image or pdf) for a volunteer on a mission.
    /// Returns the saved file url (relative) on success.
    /// 


    Task StartMissionAsync(int volunteerId, int missionId);
    Task<IEnumerable<MissionDto>> GetAllMissionsAsync();
    Task<string> SubmitMissionReportAsync(int volunteerId, int missionId, IFormFile file, string? notes = null);
    Task<GetProfileDto> GetProfileAsync(string userId);
    Task<bool> UpdateProfileAsync(string userId, UpdateProfileDto dto);
}