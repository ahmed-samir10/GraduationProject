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
        [CachedAttribute(600)]
        public async Task<ActionResult<CartResponseDto>> GetCart()
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var Ben = await _unitOfWork .Repository<Beneficiary, int>().FirstOrDefaultAsync(b => b.AppUserId == userId); 
           
            var cart = await _cartService.GetCartAsync(Ben.Id);

            return Ok(new { success = true, data = cart });
        }




        // =========================
        // Add Item To Cart
        // =========================
        [HttpPost("Additems")]
        public async Task<IActionResult> AddItemToCart([FromBody] AddToCartDto dto)
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var Ben = await _unitOfWork
                 .Repository<Beneficiary, int>()
                 .FirstOrDefaultAsync(b => b.AppUserId == userId);

          var res =   await _cartService.AddItemAsync(Ben.Id, dto);
            return Ok(new { success = true, data = res });
        }



/*
        // =========================
        // Add multiple items in one request
        // =========================
        [HttpPost("AddItemsBulk")]
        public async Task<IActionResult> AddItemsBulk([FromBody] IEnumerable<AddToCartDto> dtos)
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var beneficiary = await _unitOfWork
                    .Repository<Beneficiary, int>()
                    .FirstOrDefaultAsync(b => b.AppUserId == userId);

            await _cartService.AddItemsAsync(beneficiary.Id, dtos);
            return Ok(new { success = true, data = "تم الاضافه" });
        }
*/



        // =========================
        // Remove Item From Cart
        // =========================
        [HttpDelete("Delitems/{productId}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var Ben = await _unitOfWork
                    .Repository<Beneficiary, int>()
                    .FirstOrDefaultAsync(b => b.AppUserId == userId);

            var res= await _cartService.RemoveItemAsync(Ben.Id, productId);
            return Ok(new { success = true, data = res });
        }


/*

        // =========================
        // Remove multiple items
        // =========================
        [HttpDelete("Delitems")]
        public async Task<IActionResult> RemoveItems([FromBody] IEnumerable<int> productIds)
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var beneficiary = await _unitOfWork
                .Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            var res = await _cartService.RemoveItemsAsync(beneficiary.Id, productIds);
            return Ok(new { success = true, data = res });
        }
*/



        // =========================
        // Update multiple items quantities
        // =========================
        [HttpPut("UpdateItems")]
        public async Task<IActionResult> UpdateItems([FromBody] UpdateCartItemDto Updated)
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            var beneficiary = await _unitOfWork
                .Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            var res = await _cartService.UpdateItemQuantityAsync(beneficiary.Id, Updated.ProductId , Updated.Quantity);
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

            var res = await _cartService.ClearCartAsync(Ben.Id);
            return Ok(new { success = true, data = res });
        }
    }
}
