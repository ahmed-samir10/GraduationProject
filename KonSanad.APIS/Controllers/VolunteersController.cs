
using KonSanad.Core.Dtos;
using KonSanad.Repository.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KonSanad.APIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VolunteerController : ControllerBase
    {
        private readonly IVolunteerService _volunteerService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICampaignService _campaignService;

        public VolunteerController(IVolunteerService volunteerService, IUnitOfWork unitOfWork, ICampaignService campaignService)
        {
            _volunteerService = volunteerService;
            _unitOfWork = unitOfWork;
            _campaignService = campaignService;
        }


        // ----------------------------------------------------------
        // GET: api/Volunteer/missions
        [Authorize]
        [HttpGet("missions")]
        public async Task<IActionResult> GetMissions()
        {
            var missions = await _volunteerService.GetAllMissionsAsync();
            return Ok(new { success = true, data = missions });
        }


        [HttpGet("GetMissionsByVolunteerId")]
        public async Task<IActionResult> GetMissionsByVolunteerId()

        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;


            var Vol = await _unitOfWork
     .Repository<Volunteer, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (Vol == null)
                return BadRequest(new { Error = "No Donor found for this user" });

            int VolID = Vol.Id;


            var mission = await _volunteerService.GetMissionsByVolunteerId(VolID);
            if (mission == null)
            {
                return NotFound(new { success = false, message = "Mission not found" });
            }
            return Ok(new { success = true, data = mission });
        }


        [Authorize]
        [HttpGet("GetAllCampiagn")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1,
 [FromQuery] int pageSize = 8)
        {
            var list = await _campaignService.GetAllAsync(pageNumber,pageSize);
            return Ok(new { success = true, data = list });
        }
        // ----------------------------------------------------------



        // ----------------------------------------------------------
        // POST: api/Volunteer/ReportSubmitted
        [HttpPost("ReportSubmitted")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ReportSubmitted([FromForm] DTOs.ReportSubmittedDto dto)
        {
            
                // Basic validation of the incoming data 
                if (dto is null)
               return Ok (new { success = false, message = "Invalid data" });


            // Validate required fields 
            if (dto.File is null || dto.File.Length == 0)
                    return Ok(new { success = false, message = "File is required" });

            // Basic file validations (type and size)
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "application/pdf" };
                const long maxFileSize = 10 * 1024 * 1024; // 10 MB

                // Check file type and size 
                if (!allowedContentTypes.Contains(dto.File.ContentType))
                    return Ok (new { success = false, message = "Invalid file type. Allowed types are JPEG, PNG, GIF, PDF." });

            if (dto.File.Length > maxFileSize)
                    return Ok (new { success = false, message = "File size exceeds the maximum allowed size of 10 MB." });

            // Further validations can be added here
            // Token handling and volunteer identification
            // derive volunteer from authenticated user token (client must NOT send VolunteerId)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Ok (new { success = false, message = "User id not found in token" });

            // find volunteer by AppUserId using injected unit of work
            var volunteer = await _unitOfWork.Repository<Volunteer, int>()
                                .FirstOrDefaultAsync(v => v.AppUserId == userId);

            if (volunteer is null)
                return Ok (new { success = false, message = "Volunteer not found for the authenticated user" });

            // Call the service to handle the report submission
            var fileUrl = await _volunteerService.SubmitMissionReportAsync(volunteer.Id, dto.MissionId, dto.File, dto.Notes);


                return Ok(new { success = true, data = "تم رفع الملف" });
            

          
        }
        // ----------------------------------------------------------



        [HttpPut("update")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateMissionReport([FromForm] UpdateMissionReportDto requst)
        {
            var result = await _volunteerService
                .UpdateMissionReportAsync(requst.VolunteerId,requst.MissionId ,requst.File , requst.Notes);

            return Ok(new { success = true, data = "تم التعديل" });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteMissionReport(
      [FromQuery] int volunteerId,
      [FromQuery] int missionId)
        {
            var result = await _volunteerService
                .DeleteMissionReportAsync(volunteerId, missionId);

            return Ok(new { success = true, data = "تم الحذف" });
        }



        // ----------------------------------------------------------

        [Authorize]
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartTask(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;


            var Vol = await _unitOfWork
     .Repository<Volunteer, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (Vol == null)
                return BadRequest(new { Error = "لا يوجد متطوع" });

            int VolID = Vol.Id;
          
              var data=  await _volunteerService.StartMissionAsync(VolID,id);
               
                return Ok(new { success = true, data = data});
            
          
        }
        // ----------------------------------------------------------

        [Authorize]
        [HttpPost("join-campaign/{campaignId}")]
        public async Task<IActionResult> JoinCampaign(int campaignId)
        {
            
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;


                var Vol = await _unitOfWork
         .Repository<Volunteer, int>()
         .FirstOrDefaultAsync(b => b.AppUserId == userId);

                if (Vol == null)
                    return Ok(new { success = true, data = "المطتوع غير موجود" });

                int VolID = Vol.Id;
                var result = await _volunteerService.JoinCampaignAsync(VolID, campaignId);
                return Ok(new { success = true, data = result });
           
                
           
        }



        [Authorize]
        [HttpGet("joined-campaigns")]
        public async Task<IActionResult> GetJoinedCampaigns()
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "User id not found in token" });

            var result = await _volunteerService.GetJoinedCampaignsAsync(userId);
            if (result == null)
                return Ok(new { success = false, message = "No joined campaigns found for this user" });

            return Ok(new { success = true, data = result });
        }
        // ----------------------------------------------------------
        // profile of volunteer
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { error = "User id not found in token" });

            var result = await _volunteerService.GetProfileAsync(userId);
            return Ok(new { success = true, data = result });
        }
        // ----------------------------------------------------------


        // ----------------------------------------------------------
        // Update profile of volunteer
        [Authorize]
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { error = "User id not found in token" });

            var success = await _volunteerService.UpdateProfileAsync(userId, dto);

            return Ok(new { success, message = "Profile updated successfully" });
        }
    }
}
