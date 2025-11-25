using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;

//using KonSanad.Core.IRepositery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KonSanad.APIS.Controllers
{
    // KonSanad.APIS/Controllers/DonationOrdersController.cs
    [Route("api/donation-orders")]
    [ApiController]
    //[Authorize(Roles = "Donor")] // لو عندك JWT وRoles
    public class DonationOrdersController : ControllerBase
    {
        private readonly IDonationOrderService _donationService;

        public DonationOrdersController(IDonationOrderService donationService)
        {
            _donationService = donationService;
        }

        // ده اللي هيستخدمه الفرونت لما المتبرع يضغط "أتبرع الآن"
        [HttpPost]
        public async Task<IActionResult> CreateDonation([FromBody] CreateDonationRequestDto request)
        {
            if (request.CashAmount == null && (request.Items == null || !request.Items.Any()))
                return BadRequest(new { success = false, message = "يجب اختيار تبرع نقدي أو عيني" });

            var result = await _donationService.CreateDonationAsync(request, GetCurrentDonorId());

            return Ok(new { success = true, data = result });
        }

        // جلب تاريخ تبرعات المتبرع
        [HttpGet("my-donations")]
        public async Task<IActionResult> GetMyDonations()
        {
            var donations = await _donationService.GetDonorDonationsAsync(GetCurrentDonorId());
            return Ok(new { success = true, data = donations });
        }

        private int GetCurrentDonorId()
        {
            // هنا هتجيب الـ DonorId من الـ JWT Claim
            var donorIdClaim = User.Claims.FirstOrDefault(c => c.Type == "DonorId");
            return donorIdClaim != null ? int.Parse(donorIdClaim.Value) : 0;
        }
    }
}
