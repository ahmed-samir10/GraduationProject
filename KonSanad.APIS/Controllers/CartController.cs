using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using KonSanad.Core.Dtos;

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

        private async Task<int?> GetBeneficiaryIdAsync()
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            if (string.IsNullOrEmpty(userId)) return null;

            var ben = await _unitOfWork
                .Repository<Beneficiary, int>()
                .FirstOrDefaultAsync(b => b.AppUserId == userId);

            return ben?.Id;
        }

      

        // =========================
        // Get Cart By UserId
        // =========================
        [HttpGet]
        public async Task<ActionResult<CartResponseDto>> GetCart()
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;


            var beneficiary = await _unitOfWork
     .Repository<Beneficiary, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return Ok(new { success = false, data = " لا يوجد مستفيد" });

            int beneficiaryId = beneficiary.Id;
            var cart = await _cartService.GetCartAsync(beneficiaryId);
            return Ok(new { success = true, data = cart });
        }




        // =========================
        // Add Item To Cart
        // =========================
        [HttpPost("Additems")]
        public async Task<IActionResult> AddItemToCart([FromBody] AddToCartDto dto)
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;


            var beneficiary = await _unitOfWork
     .Repository<Beneficiary, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return Ok(new { success = false, data = " لا يوجد مستفيد" });

            int beneficiaryId = beneficiary.Id;

           var res= await _cartService.AddItemAsync(beneficiaryId, dto);
            return Ok(new { success = true, data = res });
        }




   /*     // =========================
        // Add multiple items in one request
        // =========================
        [HttpPost("AddItemsBulk")]
        public async Task<IActionResult> AddItemsBulk([FromBody] IEnumerable<AddToCartDto> dtos)
        {
            var benId = await GetBeneficiaryIdAsync();
            if (benId == null) return Unauthorized(new { success = false, message = "User not found" });

            await _cartService.AddItemsAsync(benId.Value, dtos);
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


            var beneficiary = await _unitOfWork
     .Repository<Beneficiary, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return Ok(new { success = false, data = " لا يوجد مستفيد" });

            int beneficiaryId = beneficiary.Id;
            var res= await _cartService.RemoveItemAsync(beneficiaryId, productId);
            return Ok(new { success = true, data = res });
        }



        /*
                // =========================
                // Remove multiple items
                // =========================
                [HttpDelete("Delitems")]
                public async Task<IActionResult> RemoveItems([FromBody] IEnumerable<int> productIds)
                {
                    var benId = await GetBeneficiaryIdAsync();
                    if (benId == null) return Unauthorized(new { success = false, message = "User not found" });

                    var res = await _cartService.RemoveItemsAsync(benId.Value, productIds);
                    return Ok(new { success = true, data = res });
                }
        */



        // =========================
        // Update multiple items quantities
        // =========================
        [HttpPut("UpdateItems")]
        public async Task<IActionResult> UpdateItems([FromBody] UpdateCartItemDto  Updated)
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;


            var beneficiary = await _unitOfWork
     .Repository<Beneficiary, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return Ok(new { success = false, data = " لا يوجد مستفيد" });

            int beneficiaryId = beneficiary.Id;
            var res = await _cartService.UpdateItemQuantityAsync(beneficiaryId, Updated.ProductId , Updated.Quantity);
            return Ok(new { success = true, data = res });
        }




        // =========================
        // Clear Cart
        // =========================
        [HttpDelete("ClearCart")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;


            var beneficiary = await _unitOfWork
     .Repository<Beneficiary, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return Ok(new { success = false, data = " لا يوجد مستفيد" });

            int beneficiaryId = beneficiary.Id;
            var res = await _cartService.ClearCartAsync(beneficiaryId);
            return Ok(new { success = true, data = res });
        }


        // =========================
        // Submit Cart (retrieve all cart data in one request)
        // Front-end should call this when user clicks Submit to get final cart snapshot
        // and basic validation (missing items or invalid quantities).
        // =========================
        [HttpPost("SubmitCart")]
        public async Task<IActionResult> SubmitCart()
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;


            var beneficiary = await _unitOfWork
     .Repository<Beneficiary, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return Ok(new { success = false, data = " لا يوجد مستفيد" });

            int beneficiaryId = beneficiary.Id;
            var submitResult = await _cartService.SubmitCartAsync(beneficiaryId);
            if (!submitResult.Success)
                return BadRequest(new { success = false, errors = submitResult.Errors });

            return Ok(new { success = true, message = "تم إرسال الطلب بنجاح" });
        }
    }
}
