using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KonSanad.APIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MangerController : ControllerBase
    {
        private readonly IMangerServices _mangerServices;

        public MangerController(IMangerServices mangerServices)
        {
            _mangerServices = mangerServices;
        }

        [HttpGet("help-orders")]
        public async Task<IActionResult> GetHelpOrders()
        {
            var result = await _mangerServices.GetHelpOrderAsync();

            return Ok(result);
        }
        [HttpGet("AllVolunteer")]
        public async Task<IActionResult> GetVolunteers()
        {
            var result = await _mangerServices.GetVolunteerAsync();
            return Ok(result);
        }
        [HttpPost("AssignVolunteer")]
        public async Task<IActionResult> AssignVolunteer([FromBody] AssignVolunteerRequestDto dto)
        {
            if (dto == null || dto.HelpOrderId <= 0 || dto.VolunteerId <= 0)
                return BadRequest("Invalid data");

            var result = await _mangerServices.AssignVolunteerAsync(dto.HelpOrderId, dto.VolunteerId);

            if (result.Contains("not found"))
                return NotFound(result);

            return Ok(result);
        }
    }
}
