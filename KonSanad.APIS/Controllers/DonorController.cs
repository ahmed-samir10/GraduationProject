
namespace KonSanad.APIS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DonorController : ControllerBase
{
    public ICampaignService _service { get; }
    public IUnitOfWork _unitOfWork { get; set; }
    public IDonorServices donorServices { get; }
    public DonorController(ICampaignService service, IDonorServices donorServices, IUnitOfWork unitOfWork)
    {
        _service = service;
        this.donorServices = donorServices;
        _unitOfWork = unitOfWork;
    }

    [Authorize]
    [HttpGet("GetAllCampiagn")]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 10)
    {
        var list = await _service.GetAllAsync(pageNumber, pageSize);
        return Ok(new { success = true, data = list });
    }

    /*[Authorize]
    [HttpPost("donateCash")]
    public async Task<IActionResult> Donate([FromForm] CreateDonationCashRequestDto request)
    {
        // 1. Validate the incoming request
        if (!ModelState.IsValid)
            return BadRequest(ModelState);


        // 2. Get the user ID from the JWT token
        var userId = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");


        // 3. Retrieve the donor associated with the user ID
        var donor = await _unitOfWork.Repository<Donor, int>()
            .FirstOrDefaultAsync(d => d.AppUserId == userId);


        // 4. If no donor is found, return an error response
        if (donor is null)
            return BadRequest(new { Error = "No Donor found for this user" });


        // 5. Create the cash donation using the donor's ID
        var result = await donorServices.CreateDonationCashAsync(request, donor.Id);

        // 6. Return the result of the donation creation
        return Ok(new { success = true, data = result });
    }
*/

/*    [HttpPost("donate-custom-item")]
    [Authorize]
    public async Task<IActionResult> DonateCustomItem([FromForm] CreateCustomDonationItemRequestDto request)
    {
        try
        {
            // Get user ID from token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new
                {
                    success = false,
                    message = "User not authenticated"
                });

            // Get donor by AppUserId
            var donor = await _unitOfWork.Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (donor == null)
                return NotFound(new
                {
                    success = false,
                    message = "Donor not found"
                });

            // Create custom item donation
            var result = await donorServices.CreateCustomItemDonationAsync(request, donor.Id);

            return Ok(new
            {
                success = true,
                message = result.Message,
                data = result
            });
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = "بيانات غير صحيحة: " + ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "حدث خطأ أثناء معالجة الطلب: " + ex.Message
            });
        }
    }
    //------------------------------------------------------------------*/
    [Authorize]
    [HttpGet("my-requestsItem")]
    public async Task<IActionResult> GetMyItemRequests()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { error = "User id not found in token" });

        var result = await donorServices.GetMyDonatedOrderItemsAsync(userId);

        return Ok(new { success = true, data = result });
    }
    //-------------------------------------------------------------------
    [Authorize]
    [HttpGet("my-requestsCash")]
    public async Task<IActionResult> GetMyCashRequests()
    {
        // 1. Get user ID from token
        var userId = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        // 2. Validate user ID
        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { error = "User id not found in token" });

        // 3. Get cash donations for the user
        var result = await donorServices.GetMyCashDonationsAsync(userId);

        // 4. Return the result
        return Ok(new { success = true, data = result });
    }



    [Authorize]
    [HttpPost("donateItem")]
   /* public async Task<IActionResult> Donate([FromForm] CreateDonationItemRequestDto request)

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
        return Ok(new { success = true, data = result });

    }

    [Authorize]
    [HttpGet("MyDonationsInCampaign-requests")]
    public async Task<IActionResult> GetMyDonationsInCampaignRequests()
    {
        // 1. Get user ID from token
        var userId = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        // 2. Validate user ID
        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { error = "User id not found in token" });

        // 3. Get donations in campaign requests for the user
        var result = await donorServices.GetMyDonationsInCampaignRequestsAsync(userId);

        // 4. Return the result
        return Ok(new { success = true, data = result });
    }


    // ====================== Get Profile EndPoint ==========================
    [Authorize]
    [HttpGet("GetProfile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { error = "User id not found in token" });

        var result = await donorServices.GetProfileAsync(userId);
        return Ok(new { success = true, data = result });
    }

    // ====================== Update Profile EndPoint ==========================
    [Authorize]
    [HttpPut("UpdateProfile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { error = "User id not found in token" });

        var success = await donorServices.UpdateProfileAsync(userId, dto);

        return Ok(new { success, message = "Profile updated successfully" });
    }
}

