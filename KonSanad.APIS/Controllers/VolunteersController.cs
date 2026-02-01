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

        public VolunteerController(IVolunteerService volunteerService)
        {
            _volunteerService = volunteerService;
        }

        // GET: api/Volunteer/missions
        [Authorize]
        [HttpGet("missions")]
        public async Task<IActionResult> GetMissions()
        {
            var missions = await _volunteerService.GetAllMissionsAsync();
            return Ok(missions);
        }
    }
}