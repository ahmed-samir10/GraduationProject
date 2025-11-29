//using KonSanad.Core.Dtos;
//using KonSanad.Core.IRepositery;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace KonSanad.APIS.Controllers
//{
//    [Route("api/donation-orders")]
//    [ApiController]

//    [Authorize]
//    public class DonationOrdersController : ControllerBase
//    {
//        private readonly IDonationOrderService _donationService;

//        public DonationOrdersController(IDonationOrderService donationService)
//        {
//            _donationService = donationService;
//        }

//        // donate to campaign
//        [HttpPost("campaign/{campaignId}")]
//        public async Task<IActionResult> CreateDonationToCampaign(int campaignId, [FromBody] CreateDonationRequestDto request)
//        {
//            if (request == null) return BadRequest(new { success = false, message = "Invalid request" });
//            if (request.CashAmount == null && (request.Items == null || !request.Items.Any()))
//                return BadRequest(new { success = false, message = "يجب اختيار تبرع نقدي أو عيني" });

//            var result = await _donationService.CreateDonationToCampaignAsync(request, GetCurrentDonorId(), campaignId);
//            return Ok(new { success = true, data = result });
//        }

//        // donate to beneficiary
//        [HttpPost("beneficiary/{beneficiaryId}")]
//        public async Task<IActionResult> CreateDonationToBeneficiary(int beneficiaryId, [FromBody] CreateDonationRequestDto request)
//        {
//            if (request == null) return BadRequest(new { success = false, message = "Invalid request" });
//            if (request.CashAmount == null && (request.Items == null || !request.Items.Any()))
//                return BadRequest(new { success = false, message = "يجب اختيار تبرع نقدي أو عيني" });

//            var result = await _donationService.CreateDonationToBeneficiaryAsync(request, GetCurrentDonorId(), beneficiaryId);
//            return Ok(new { success = true, data = result });
//        }

//        // existing endpoint kept for backward compatibility (optional)
//        [HttpPost]
//        public async Task<IActionResult> CreateDonation([FromBody] CreateDonationRequestDto request)
//        {
//            if (request == null) return BadRequest(new { success = false, message = "Invalid request" });
//            if (request.CashAmount == null && (request.Items == null || !request.Items.Any()))
//                return BadRequest(new { success = false, message = "يجب اختيار تبرع نقدي أو عيني" });

//            var result = await _donationService.CreateDonationAsync(request, GetCurrentDonorId());
//            return Ok(new { success = true, data = result });
//        }

//        [HttpGet("my-donations")]
//        public async Task<IActionResult> GetMyDonations()
//        {
//            var donations = await _donationService.GetDonorDonationsAsync(GetCurrentDonorId());
//            return Ok(new { success = true, data = donations });
//        }

//        private int GetCurrentDonorId()
//        {
//            var donorIdClaim = User.Claims.FirstOrDefault(c => c.Type == "DonorId");
//            return donorIdClaim != null ? int.Parse(donorIdClaim.Value) : 0;
//        }
//    }
//}
