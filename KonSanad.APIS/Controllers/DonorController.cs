using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using KonSanad.Repository.Repository;
using KonSanad.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KonSanad.APIS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonorController : ControllerBase
     {
       public ICampaignService _service { get; }
        public IUnitOfWork _unitOfWork { get; set; }
        public IDonorServices donorServices { get; }
        public DonorController(ICampaignService service, IDonorServices donorServices , IUnitOfWork unitOfWork)
        {
            _service = service;
            this.donorServices = donorServices;
            _unitOfWork = unitOfWork;
        }

          [Authorize]
        [HttpGet("GetAllCampiagn")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(new { success = true, data = list });
        }

        [Authorize]
        [HttpPost("donateCash")]
        public async Task<IActionResult> Donate([FromForm] CreateDonationCashRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;


            var Donor = await _unitOfWork
     .Repository<Donor, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (Donor == null)
                return BadRequest(new { Error = "No Donor found for this user" });

            int DonorID = Donor.Id;

            var result = await donorServices.CreateDonationCashAsync(request, DonorID);

            return Ok(new { success = true, data = result });
        }


       
        [Authorize]
        [HttpPost("donateItem")]
        public async Task<IActionResult> Donate([FromForm] CreateDonationItemRequestDto request )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            
            var Donor = await _unitOfWork
     .Repository<Donor, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (Donor == null)
                return BadRequest(new { Error = "No Donor found for this user" });

            int DonorID = Donor.Id;

            var result = await donorServices.CreateDonationItemAsync(request, DonorID);

            return Ok(new { success = true, data = result });
        }
         */
        [HttpGet("low-stock-supplies")]
        public async Task<IActionResult> GetLowStockSupplies()
        {
            var result = await donorServices.GetLowStockSuppliesAsync();
           return    Ok(new { success = true, data = result });
          
        }


    }
}
