
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

    [Authorize]
    [HttpGet("my-requestsItem")]
    public async Task<IActionResult> GetMyItemRequests()
    {
        // 1. Get user ID from token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // 2. Validate user ID
        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { error = "User id not found in token" });

        // 3. Get donated items for the user
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
        var result = await donorServices.GetMyCashDonationsAsyncc(userId);

        // 4. Return the result
        return Ok(new { success = true, data = result });
    }



    

    [HttpGet("low-stock-supplies")]
    public async Task<IActionResult> GetLowStockSupplies(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 8)
    {
        var result = await donorServices.GetLowStockSuppliesAsync(pageNumber, pageSize);

        return Ok(new
        {
            success = true,
            data = result
        });
    }
    //--------------------------------------------------------------------------------
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

    [HttpPut("UpdateAmount")]
    public async Task<IActionResult> UpdateAmount(int id, decimal amount)
    {
        await donorServices.UpdateCashDonationAmountAsync(id, amount);

        return Ok(new
        {
            success = true,
            message = "تم التحديث بنجاح"
        });
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCashDonation(int id)
    {
        await donorServices.DeleteCashDonationAsync(id);

        return Ok(new
        {
            success = true,
            message = "Donation deleted successfully"
        });
    }

    [Authorize]
    [HttpPut("update-item-quantity")]
    public async Task<IActionResult> UpdateItemQuantity(int orderItemId, int newQuantity)
    {
        var result = await donorServices.UpdateItemQuantityAsync(orderItemId, newQuantity);

        if (!result.Success)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    [Authorize]
    [HttpDelete("delete-item")]
    public async Task<IActionResult> DeleteItem(int orderItemId)
    {
        var result = await donorServices.DeleteItemAsync(orderItemId);

        if (!result.Success)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message });
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

