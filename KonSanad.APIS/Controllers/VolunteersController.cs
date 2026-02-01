
using KonSanad.Core.Dtos;
using KonSanad.Repository.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KonSanad.APIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VolunteerController : ControllerBase
    {
        private readonly IVolunteerService _volunteerService;
        private readonly IUnitOfWork _unitOfWork;

        public VolunteerController(IVolunteerService volunteerService , IUnitOfWork unitOfWork)
        {
            _volunteerService = volunteerService;
            _unitOfWork = unitOfWork;
        }

        

        // GET: api/Volunteer/missions
        [Authorize]
        [HttpGet("missions")]
        public async Task<IActionResult> GetMissions()
        {
            var missions = await _volunteerService.GetAllMissionsAsync();
            return Ok(missions);
        }

        [HttpPost("ReportSubmitted")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ReportSubmitted([FromForm] DTOs.ReportSubmittedDto dto)
        {
            // Basic validation of the incoming data 
            if (dto is null)
                return BadRequest(new { success = false, message = "Invalid payload" });


            // Validate required fields 
            if (dto.File is null || dto.File.Length == 0)
                return BadRequest(new { success = false, message = "File is required" });

            // Basic file validations (type and size)
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "application/pdf" };
            const long maxFileSize = 10 * 1024 * 1024; // 10 MB

            // Check file type and size 
            if (!allowedContentTypes.Contains(dto.File.ContentType))
                return BadRequest(new { success = false, message = "Unsupported file type. Allowed: jpeg, png, gif, pdf" });

            if (dto.File.Length > maxFileSize)
                return BadRequest(new { success = false, message = "File too large. Maximum allowed size is 10MB." });

            // Further validations can be added here
            // Token handling and volunteer identification
           // derive volunteer from authenticated user token (client must NOT send VolunteerId)
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                // find volunteer by AppUserId using injected unit of work
                var volunteer = await _unitOfWork.Repository<Volunteer, int>()
                                    .FirstOrDefaultAsync(v => v.AppUserId == userId);

                if (volunteer is null)
                    return BadRequest(new { success = false, message = "Volunteer not found for authenticated user" });

                // Call the service to handle the report submission
                var fileUrl = await _volunteerService.SubmitMissionReportAsync(volunteer.Id, dto.MissionId, dto.File, dto.Notes);


                return Ok(new { success = true, data = new { fileUrl } });
           
           /* // Handle specific exceptions as needed 
          *//*  catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            // General exception handling
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }*/
        }

        [Authorize]
        [HttpPut("{id}/start")]
        public async Task<IActionResult> StartTask(int id)
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
            try
            {
                await _volunteerService.StartMissionAsync(VolID,id);
                return Ok("Mission started successfully");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message); // هيرجع 400 مع الـ message
            }
        }



    }
}
