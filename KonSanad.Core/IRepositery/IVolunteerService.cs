using KonSanad.Core.Dtos;
using KonSanad.Core.Entity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace KonSanad.Core.IRepositery;

public interface IVolunteerService
{
    /// Submit a report file (image or pdf) for a volunteer on a mission.
    /// Returns the saved file url (relative) on success.
    /// 


    Task StartMissionAsync(int volunteerId, int missionId);
    Task<IEnumerable<MissionDto>> GetAllMissionsAsync();
    Task<IEnumerable<VolunteerMissionDto>> GetMissionsByVolunteerId(int volunteerId);
    Task<string> JoinCampaignAsync(int volunteerId, int campaignId);
    Task<string> SubmitMissionReportAsync(int volunteerId, int missionId, IFormFile file, string? notes = null);
}