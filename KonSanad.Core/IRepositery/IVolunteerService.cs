using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
namespace KonSanad.Core.IRepositery;

public interface IVolunteerService
{
    /// Submit a report file (image or pdf) for a volunteer on a mission.
    /// Returns the saved file url (relative) on success.
    Task<string> SubmitMissionReportAsync(int volunteerId, int missionId, IFormFile file, string? notes = null);
}