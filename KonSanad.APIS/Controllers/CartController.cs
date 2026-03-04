using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly KonSanadDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public CartController(ICartService cartService , KonSanadDbContext context , IUnitOfWork unitOfWork)
    {
        _cartService = cartService;
        _context = context;
        _unitOfWork = unitOfWork;
    }
    private async Task<(int userId, string role)?> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
       // var role = User.FindFirst("Role")?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            return null;

        int id;

        if (role.Equals("Beneficiary", StringComparison.OrdinalIgnoreCase))
        {
            var beneficiary = await _unitOfWork
                .Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return null;

            id = beneficiary.Id;
        }
        else if (role.Equals("Donor", StringComparison.OrdinalIgnoreCase))
        {
            var donor = await _unitOfWork
                .Repository<Donor, int>()
                .FirstOrDefaultAsync(d => d.AppUserId == userId);

            if (donor == null)
                return null;

            id = donor.Id;
        }
        else
        {
            return null;
        }

        return (id, role.ToLower());
    }
    // =========================
    // Get Cart
    // =========================
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var user = await GetCurrentUser();
        if (user == null) return Unauthorized();

        var cart = await _cartService.GetCartAsync(user.Value.userId, user.Value.role);
        return Ok(new { success = true, data = cart });
    }

    // =========================
    // Add Beneficiary Item
    // =========================
    [HttpPost("Additems")]
    [Authorize(Roles = "Beneficiary")]
    public async Task<IActionResult> AddItemToCart([FromBody] AddToCartDto dto)
    {
        var user = await GetCurrentUser();
        if (user == null) return Unauthorized();

        var result = await _cartService.AddToCartAsync(user.Value.userId, user.Value.role, dto);

        return Ok(new { success = true, message = result });
    }

    // =========================
    // Add Donation Item
    // =========================
    [HttpPost("add-donation-item")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> AddDonationItem([FromForm] CreateDonationItemRequestDto dto)
    {
        var user = await GetCurrentUser();
        if (user == null) return Unauthorized();

        var result = await _cartService.AddToCartAsync(user.Value.userId, user.Value.role, dto);

        return Ok(new { success = true, message = result });
    }

    // =========================
    // Add Donation Cash
    // =========================
    [HttpPost("add-donation-cash")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> AddDonationCash([FromBody] CreateDonationCashRequestDto dto)
    {
        var user = await GetCurrentUser();
        if (user == null) return Unauthorized();

        var result = await _cartService.AddToCartAsync(user.Value.userId, user.Value.role, dto);

        return Ok(new { success = true, message = result });
    }

    // =========================
    // Add Custom Donation Item
    // =========================
    [HttpPost("add-custom-item")]
    [Authorize(Roles = "Donor")]
    public async Task<IActionResult> AddCustomItem([FromForm] CreateCustomDonationItemRequestDto dto)
    {
        var user = await GetCurrentUser();
        if (user == null) return Unauthorized();

        var result = await _cartService.AddToCartAsync(user.Value.userId, user.Value.role, dto);

        return Ok(new { success = true, message = result });
    }

    // =========================
    // Update Quantity
    // =========================
    [HttpPut("UpdateItems")]
    public async Task<IActionResult> UpdateItems(int productId, [FromQuery] int quantity)
    {
        var user = await GetCurrentUser();
        if (user == null) return Unauthorized();

        var result = await _cartService.UpdateItemAsync(user.Value.userId, user.Value.role, productId, quantity);

        return Ok(new { success = true, message = result });
    }

    // =========================
    // Remove Item
    // =========================
    [HttpDelete("Delitems/{productId}")]
    public async Task<IActionResult> RemoRemoveItem(int productId)
    {
        var user = await GetCurrentUser();
        if (user == null) return Unauthorized();

        var result = await _cartService.RemoveItemAsync(user.Value.userId, user.Value.role, productId);

        return Ok(new { success = true, message = result });
    }

    // =========================
    // Clear Cart
    // =========================
    [HttpDelete("ClearCart")]
    public async Task<IActionResult> ClearCart()
    {
        var user = await GetCurrentUser();
        if (user == null) return Unauthorized();

        await _cartService.ClearCartAsync(user.Value.userId, user.Value.role);

        return Ok(new { success = true, message = "تم مسح الكارت" });
    }

    // =========================
    // Submit Cart
    // =========================
    [HttpPost("SubmitCart")]
    public async Task<IActionResult> SubmitCart()
    {
        var user = await GetCurrentUser();
        if (user == null) return Unauthorized();

        var result = await _cartService.SubmitCartAsync(user.Value.userId, user.Value.role);

        if (!result.Success)
            return BadRequest(result);

        return Ok(new { success = true, message = "تم الإرسال بنجاح" });
    }
}