using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KonSanad.APIS.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        #region 📊 Dashboard

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var result = await _reportService.GetDashboardAsync();
            return Ok(result);
        }

        #endregion

        #region 👥 Donors Reports

        [HttpGet("donors/last-month")]
        public async Task<IActionResult> DonorsLastMonth()
        {
            var result = await _reportService.GetDonorsLastMonthAsync();
            return Ok(result);
        }

        [HttpGet("donors/top")]
        public async Task<IActionResult> TopDonors([FromQuery] int top = 10)
        {
            var result = await _reportService.GetTopDonorsAsync(top);
            return Ok(result);
        }

        [HttpGet("donors/between-dates")]
        public async Task<IActionResult> DonationsBetweenDates(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var result = await _reportService.GetDonationsBetweenDatesAsync(from, to);
            return Ok(result);
        }

        #endregion

        #region 🏕 Campaign Reports

        [HttpGet("campaigns/active")]
        public async Task<IActionResult> ActiveCampaigns()
        {
            var result = await _reportService.GetActiveCampaignsAsync();
            return Ok(result);
        }

        [HttpGet("campaigns/expired")]
        public async Task<IActionResult> ExpiredCampaigns()
        {
            var result = await _reportService.GetExpiredCampaignsAsync();
            return Ok(result);
        }

       

        #endregion

        #region 🆘 Help Requests Reports

        [HttpGet("requests/pending")]
        public async Task<IActionResult> PendingRequests()
        {
            var result = await _reportService.GetPendingRequestsAsync();
            return Ok(result);
        }

        [HttpGet("requests/completed")]
        public async Task<IActionResult> CompletedRequests()
        {
            var result = await _reportService.GetCompletedRequestsAsync();
            return Ok(result);
        }


        #endregion

        #region 📤 Export Reports

        [HttpGet("export/donors/pdf")]
        public async Task<IActionResult> ExportDonorsPdf()
        {
            var file = await _reportService.ExportDonorsPdfAsync();
            return File(file, "application/pdf", "donors-report.pdf");
        }


        #endregion
    }
}
