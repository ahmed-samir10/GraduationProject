using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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

            return Ok(new { success = true, data = result });
        }
        [HttpGet("AllVolunteer")]
        public async Task<IActionResult> GetVolunteers()
        {
            var result = await _mangerServices.GetVolunteerAsync();
            return Ok(new { success = true, data = result });
        }


        [HttpPost("AssignVolunteer")]
        public async Task<IActionResult> AssignVolunteer([FromBody] AssignVolunteerRequestDto dto)
        {
            if (dto == null || dto.HelpOrderId <= 0 || dto.VolunteerId <= 0)
                return Ok(new { success = false, data = "Invaild Data" });

            var result = await _mangerServices.AssignVolunteerAsync(dto.HelpOrderId, dto.VolunteerId);

            if (result.Contains("not found"))
                return Ok(new { success = false, data = "not found" });

            return Ok(new { success = true, data = result });
        }

        [HttpGet("view-report/{helpOrderId}")]
        public async Task<IActionResult> ViewReport(int helpOrderId)
        {
            var result = await _mangerServices.ViewReport(helpOrderId);

            if (result == null)
           return Ok(new { success = false, data = "Report not found" });

            return Ok(new { success = true, data = result });
        }

        [HttpPost("accept/{missionId}")]
        public async Task<IActionResult> AcceptOrder(int missionId)
        {
            if (missionId <= 0)
                return BadRequest("Invalid missionId");

            var result = await _mangerServices.AcceptOrder(missionId);

            if (result.Contains("not found"))
                return NotFound(result);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        // ✅ Reject Order
        [HttpPost("reject/{missionId}")]
        public async Task<IActionResult> RejectOrder(int missionId)
        {
            if (missionId <= 0)
                return BadRequest("Invalid missionId");

            var result = await _mangerServices.RejectOrder(missionId);

            if (result.Contains("not found"))
                return NotFound(result);

            return Ok(new
            {
                success = true,
                data = result
            });
        }
    }
}
