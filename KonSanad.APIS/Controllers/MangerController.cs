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
        public async Task<IActionResult> GetHelpOrders(int pageNumber = 1, int pageSize = 8)
        {
            var result = await _mangerServices.GetHelpOrderAsync(pageNumber, pageSize);

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
                return Ok(new { success = false, massage = "Invaild Data" });

            var result = await _mangerServices.AssignVolunteerAsync(dto.HelpOrderId, dto.VolunteerId);

            if (result.Contains("not found"))
                return Ok(new { success = false, massage = "not found" });

            return Ok(new { success = true, data = result });
        }

        [HttpGet("view-report/{helpOrderId}")]
        public async Task<IActionResult> ViewReport(int helpOrderId)
        {
            var result = await _mangerServices.ViewReport(helpOrderId);

            if (result == null)
                return Ok(new { success = false, massage = "Report not found" });

            return Ok(new { success = true, data = result });
        }

        [HttpPost("accept/{missionId}")]
        public async Task<IActionResult> AcceptOrder(int missionId)
        {
            if (missionId <= 0)
               
            return Ok(new { success = false, massage = "Invalid missionId" });

            var result = await _mangerServices.AcceptOrder(missionId);

            if (result.Contains("not found"))
               return Ok(new { success = false, massage = "not found" });

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
               return Ok(new { success = false, massage = "Invalid missionId" });

            var result = await _mangerServices.RejectOrder(missionId);

            if (result.Contains("not found"))
             return Ok(new { success = false, massage = "not found" });

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpGet("join-volunteers")]
        public async Task<IActionResult> GetJoinVolunteers()
        {
            var result = await _mangerServices.GetJoinVolunteer();

            if (result == null || !result.Any())
              return Ok(new { success = false, massage = "No join volunteer requests found" });

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        [HttpPost("accept-volunteer/{volunteerId}")]
        public async Task<IActionResult> AcceptVolunteer(int volunteerId)
        {
            if (volunteerId <= 0)
               return Ok(new { success = false, massage = "Invalid volunteerId" });
            var result = await _mangerServices.AcceptVolunteer(volunteerId);
            if (result.Contains("not found"))
               return Ok(new { success = false, massage = "not found" });
            return Ok(new
            {
                success = true,
                data = result
            });
        }
        [HttpPost("reject-volunteer/{volunteerId}")]
        public async Task<IActionResult> RejectVolunteer(int volunteerId)
        {
            if (volunteerId <= 0)
                return Ok(new { success = false, massage = "Invalid volunteerId" });
            var result = await _mangerServices.RejectVolunteer(volunteerId);
            if (result.Contains("not found"))
                return Ok(new { success = false, massage = "not found" });
            return Ok(new
            {
                success = true,
                data = result
            });
        }
        [HttpGet("volunteer-requests")]
        public async Task<IActionResult> GetAllVolunteerRequests()
        {
            var result = await _mangerServices.GetAllVolunteerRequestsAsync();
          return Ok(new
            {
                success = true,
                data = result
            });
        }
        // ✅ Accept Request
        [HttpPost("accept-request")]
        public async Task<IActionResult> AcceptRequest(int requestType, int referenceId)
        {
            if (referenceId <= 0)
               return Ok (new { success = false, message = "Invalid referenceId" });

            var result = await _mangerServices.AcceptRequst(requestType, referenceId);

            if (result.Contains("not found") || result.Contains("Invalid"))
                return Ok (new { success = false, message = result });

            return Ok(new
            {
                success = true,
                message = result
            });
        }

        // ✅ Reject Request
        [HttpPost("reject-request")]
        public async Task<IActionResult> RejectRequest(int requestType, int referenceId)
        {
            if (referenceId <= 0)
               return Ok (new { success = false, message = "Invalid referenceId" });

            var result = await _mangerServices.RejectRequst(requestType, referenceId);

            if (result.Contains("not found") || result.Contains("Invalid"))
               return Ok (new { success = false, message = result });

            return Ok(new
            {
                success = true,
                message = result
            });
        }
    }
}
