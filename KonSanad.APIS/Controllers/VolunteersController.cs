using KonSanad.Core.IServices;
using KonSanad.Core.Dtos;
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
            await _volunteerService.StartMissionAsync(VolID, id);
            return Ok(new { message = "تم بدء المهمة بنجاح" });
        }


    }
}