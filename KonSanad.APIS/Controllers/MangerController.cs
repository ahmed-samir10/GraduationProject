using KonSanad.Core.Entity.Enums;
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
        public async Task<IActionResult> GetHelpOrders(
            int pageNumber = 1,
            int pageSize = 8,
            string? search = null,           // ✅ سيرش في UserName و Title و PhoneNumber و Address
            RequestedStatus? status = null,  // ✅ فيلتر بالحالة
            int? helpOrderId = null)         // ✅ فيلتر بالـ HelpOrderId
        {
            var result = await _mangerServices.GetHelpOrderAsync(pageNumber, pageSize, search, status, helpOrderId);
            return Ok(new { success = true, data = result });
        }
        //---------------------------------------------------------------------------------------
        [HttpGet("AllVolunteer")]
        public async Task<IActionResult> GetVolunteers(
     string? search = null,          // اسم المتطوع أو موقعه أو مهارته
     RequestedStatus? status = null, // حالة الطلب (Rejected/Pending/Approved) - لو null هيجيب الكل
     int pageNumber = 1,             // رقم الصفحة
     int pageSize = 8)               // عدد العناصر في الصفحة
        {
            var result = await _mangerServices.GetVolunteerAsync(search, status, pageNumber, pageSize);
            return Ok(new { success = true, data = result });
        }
        //---------------------------------------------------------------------------------------

        [HttpPost("AssignVolunteer")]
        public async Task<IActionResult> AssignVolunteer([FromBody] AssignVolunteerRequestDto dto , [FromQuery] DateTime  StartDate, [FromQuery] DateTime EndDate)
        {
            if (dto == null || dto.HelpOrderId <= 0 || dto.VolunteerId <= 0)
                return Ok(new { success = false, massage = "برجاء التأكد من البيانات المدخلة" });

            var result = await _mangerServices.AssignVolunteerAsync(dto.HelpOrderId, dto.VolunteerId , StartDate , EndDate) ;

            if (result.Contains("not found"))
                return Ok(new { success = false, massage = "لم يتم العثور على البيانات المطلوبة" });

            return Ok(new { success = true, data = result });
        }
        //---------------------------------------------------------------------------------------

        [HttpGet("view-report/{helpOrderId}")]
        public async Task<IActionResult> ViewReport(int helpOrderId)
        {
            var result = await _mangerServices.ViewReport(helpOrderId);

            if (result == null)
                return Ok(new { success = false, massage = "لم يتم العثور على التقرير" });

            return Ok(new { success = true, data = result });
        }
        //---------------------------------------------------------------------------------------

        [HttpPost("accept/{missionId}")]
        public async Task<IActionResult> AcceptOrder(int missionId)
        {
            if (missionId <= 0)
               
            return Ok(new { success = false, massage = "برجاء التأكد من البيانات المدخلة" });

            var result = await _mangerServices.AcceptOrder(missionId);

            if (result.Contains("not found"))
               return Ok(new { success = false, massage = "not found" });

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        //---------------------------------------------------------------------------------------

        // ✅ Reject Order
        [HttpPost("reject/{missionId}")]
        public async Task<IActionResult> RejectOrder(int missionId)
        {
            if (missionId <= 0)
               return Ok(new { success = false, massage = "برجاء التأكد من البيانات المدخلة" });

            var result = await _mangerServices.RejectOrder(missionId);

            if (result.Contains("not found"))
             return Ok(new { success = false, massage = "not found" });

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        //---------------------------------------------------------------------------------------

        [HttpGet("join-volunteers")]
        public async Task<IActionResult> GetJoinVolunteers(
    int pageNumber = 1,
    int pageSize = 8,
    RequestedStatus? status = null) // ✅ NEW: إضافة فلتر اختياري للـ status
        {
            var result = await _mangerServices.GetJoinVolunteer(pageNumber, pageSize, status); // ✅ UPDATED: بعتنا الـ status للـ service

            if (result == null || !result.Any())
              return Ok(new { success = false, massage = "No join volunteer requests found" });

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        //---------------------------------------------------------------------------------------
        [HttpPost("accept-volunteer/{volunteerId}")]
        public async Task<IActionResult> AcceptVolunteer(int volunteerId)
        {
            if (volunteerId <= 0)
               return Ok(new { success = false, massage = "برجاء التأكد من البيانات المدخلة" });
            var result = await _mangerServices.AcceptVolunteer(volunteerId);
            if (result.Contains("not found"))
               return Ok(new { success = false, massage = "not found" });
            return Ok(new
            {
                success = true,
                data = result
            });
        }
        //---------------------------------------------------------------------------------------
        [HttpPost("reject-volunteer/{volunteerId}")]
        public async Task<IActionResult> RejectVolunteer(int volunteerId , string  ReajectReason)
        {
            if (volunteerId <= 0)
                return Ok(new { success = false, massage = "برجاء التأكد من البيانات المدخلة" });
            var result = await _mangerServices.RejectVolunteer(volunteerId , ReajectReason);
            if (result.Contains("not found"))
                return Ok(new { success = false, massage = "not found" });
            return Ok(new
            {
                success = true,
                data = result
            });
        }
        [HttpGet("volunteer-requests")]
        public async Task<IActionResult> GetAllVolunteerRequests(
            int pageNumber = 1,
            int pageSize = 8,
            string? search = null,                          // سيرش في كل حقول الـ DTO
            RefType? requestType = null,                    // فيلتر 1: Campaign=1 or Mission=2
            VolunteerMissionStatus? missionStatus = null)   // فيلتر 2: حالة الطلب من الـ Enum
        {
            var result = await _mangerServices.GetAllVolunteerRequestsAsync(
                pageNumber, pageSize, search, requestType, missionStatus);

            return Ok(new { success = true, data = result });
        }
        [HttpGet("{referenceId}/details")]
        public async Task<IActionResult> GetRequestDetails(
    int referenceId,
    int type)
        {
            var result = await _mangerServices
                .GetRequestDetailsAsync(referenceId, type);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        //---------------------------------------------------------------------------------------


        // ✅ Accept Request
        [HttpPost("accept-request")]
        public async Task<IActionResult> AcceptRequest(int requestType, int referenceId)
        {
            if (referenceId <= 0)
               return Ok (new { success = false, message = "برجاء التأكد من البيانات المدخلة" });

            var result = await _mangerServices.AcceptRequst(requestType, referenceId);

            if (result.Contains("not found") || result.Contains("Invalid"))
                return Ok (new { success = false, message = result });

            return Ok(new
            {
                success = true,
                message = result
            });
        }
        //---------------------------------------------------------------------------------------

        // ✅ Reject Request
        [HttpPost("reject-request")]
        public async Task<IActionResult> RejectRequest(int requestType, int referenceId, string ReajectReason)
        {
            if (referenceId <= 0)
               return Ok (new { success = false, message = "برجاء التأكد من البيانات المدخلة" });

            var result = await _mangerServices.RejectRequst(requestType, referenceId , ReajectReason);

            if (result.Contains("not found") || result.Contains("Invalid"))
               return Ok (new { success = false, message = result });

            return Ok(new
            {
                success = true,
                message = result
            });
        }
        //---------------------------------------------------------------------------------------

        // GET: api/Donation
        [HttpGet("Donations")]
        public async Task<IActionResult> GetAllDonations(
      int pageNumber = 1,
      int pageSize = 10,
      string? search = null,                    // سيرش في: اسم المتبرع، اسم المنتج، اسم الحملة، المبلغ
      DonationCategory? category = null,        // فيلتر 1: Cash=1 أو Item=2
      RequestedStatus? status = null)           // فيلتر 2: Rejected=1, Pending=2, Approved=3
        {
            var result = await _mangerServices.GetDonationAsync(
                pageNumber, pageSize, search, category, status);

            return Ok(new { success = true, data = result });
        }
        //-------------------------------------------------------------------------------------------
        [HttpGet("Donation-Details/{id}")]
        public async Task<IActionResult> DonationDetails(int id)
        {
            var result = await _mangerServices.ViewAllDetailsDonation(id);

            if (result == null)
                return NotFound("Donation not found");

            return Ok(result);
        }
        //---------------------------------------------------------------------------------------------

        [HttpPost("make-manager/{volunteerId}")]
        public async Task<IActionResult> MakeManager(int volunteerId)
        {
            var result = await _mangerServices.MakeManagerAsync(volunteerId);

            if (result.Contains("not found"))
                return NotFound(result);

            return Ok(result);
        }
        [HttpPost("accept-donation/{DonationOrderId}")]
        public async Task<IActionResult> AcceptDonation(int DonationOrderId)
        {
            var result = await _mangerServices.AcceptDonation(DonationOrderId);
            if (result != "تم الموافقة على التبرع بنجاح")
                return BadRequest(new { success = false, message = result });

            return Ok(new { success = true, message = result });
        }
        //------------------------------------------------------------------------------------------------

        [HttpPost("reject-donation/{DonationOrderId}")]
        public async Task<IActionResult> RejectDonation(int DonationOrderId, [FromQuery] string RejectReason)
        {
            if (string.IsNullOrEmpty(RejectReason))
                return BadRequest(new { success = false, message = "لازم تكتب سبب الرفض" });

            var result = await _mangerServices.RejectDonation(DonationOrderId, RejectReason);

            if (result != "تم رفض التبرع ")
                return BadRequest(new { success = false, message = result });

            return Ok(new { success = true, message = result });
        }


    }
}
