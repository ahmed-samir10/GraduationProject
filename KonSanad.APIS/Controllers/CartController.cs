using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KonSanad.APIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly IUnitOfWork _unitOfWork;

        public CartController(ICartService cartService , IUnitOfWork unitOfWork)
        {
            _cartService = cartService;
            _unitOfWork = unitOfWork;
        }

      

        // =========================
        // Get Cart By UserId
        // =========================
        [HttpGet]
        public async Task<ActionResult<CartResponseDto>> GetCart()

        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var Ben = await _unitOfWork
     .Repository<Beneficiary, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);



            
            var cart = await _cartService.GetCartAsync(Ben.Id);
            return Ok(new { success = true, data = cart });
        }

        // =========================
        // Add Item To Cart
        // =========================
        [HttpPost("Additems")]
        public async Task<IActionResult> AddItemToCart(
           
            [FromBody] AddToCartDto dto)
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var Ben = await _unitOfWork
     .Repository<Beneficiary, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            await _cartService.AddItemAsync(Ben.Id, dto);
            return Ok(new { success = true, data = "تم الاضافه" });
        }

        // =========================
        // Remove Item From Cart
        // =========================
        [HttpDelete("Delitems/{productId}")]
        public async Task<IActionResult> RemoveItem(
           
            int productId)
        {

            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var Ben = await _unitOfWork
     .Repository<Beneficiary, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

      var res=      await _cartService.RemoveItemAsync(Ben.Id, productId);
            return Ok(new { success = true, data = res });
        }

        // =========================
        // Clear Cart
        // =========================
        [HttpDelete("ClearCart")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var Ben = await _unitOfWork
     .Repository<Beneficiary, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

         var res=   await _cartService.ClearCartAsync(Ben.Id);
            return Ok(new { success = true, data = res });
        }
    }
}
