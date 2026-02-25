using AutoMapper;
using Azure.Core;
using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using KonSanad.Repository.Repository;
using KonSanad.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KonSanad.APIS.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class BeneificeryController : ControllerBase
    {
        private readonly ICampaignService _service;
        private readonly IMapper _mapper;
        private readonly IHelpRequestService _helpRequestService;
        private readonly IDonationService _ORder;

        private readonly IUnitOfWork _unitOfWork;
        public BeneificeryController(ICampaignService service, IMapper mapper, IDonationService ORder, IUnitOfWork unitOfWork, IHelpRequestService helpRequestService)
        {
            _service = service;
            _mapper = mapper;
            _helpRequestService = helpRequestService;
            _ORder = ORder;
            _unitOfWork = unitOfWork;
        }


       
        [Authorize]
        [HttpGet("GetAllCampiagn")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 8)
        {
            var list = await _service.GetAllCampaignBeneficiaryAsync(pageNumber,pageSize);
            return Ok(new { success = true, data = list } );
        }




        [Authorize]
        [HttpGet("GetCampaignById")]
        public async Task<ActionResult<CampaignDto>> GetById(int id)
        {
            var campaignDto = await _service.GetByIdAsync(id);

            if (campaignDto == null)
       return Ok(new { success = false, data = $"الحملة رقم {id} غير موجودة" });


            return Ok(new { success = true, data = campaignDto });
        }




        [Authorize]
        [HttpPost("join-campaign")]
        public async Task<IActionResult> JoinCampaign( [FromBody] JoinCampaignRequestDto request)
        {

            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;


            var beneficiary = await _unitOfWork
     .Repository<Beneficiary, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return Ok(new { success = false, data = " لا يوجد مستفيد" });

            int beneficiaryId = beneficiary.Id;
            //   Console.WriteLine($"User ID from token: {userId}  , type {userId.GetType()}");
          
                var message = await _helpRequestService.JoinCampaignAsync(beneficiaryId, request);
                return Ok(new { success = true, data = message });
            
           
        }




        [Authorize]
        [HttpGet("order-item")]
        public async Task<IActionResult> GetOrderItem([FromQuery] int pageNumber = 1 , [FromQuery] int pageSize = 8)
        {
            var orderItem = await _ORder.GetAllDonationItemsAsync(pageNumber, pageSize);

            if (orderItem == null)
         
            return Ok(new { success = false, data = "الصنف غير موجود" });

            //var dto = _mapper.Map<OrderItemDto>(orderItem);

            return Ok(new { success = true, data = orderItem });
        }




        [Authorize]
        [HttpGet("order-item/{id:int}")]
        public async Task<IActionResult> GetOrderItemById(int id)
        {
            var item = await _ORder.GetOrderItemByIdAsync(id);

            if (item == null)
        
            return Ok(new { success = false, data = $"الصنف رقم {id} غير موجود أو تم حذفه" });

            return Ok(new { success = true, data = item });
        }
        // 1) Request an existing donated product (only reason required)
        // POST api/beneificery/{beneficiaryId}/request-existing




        [Authorize]
        [HttpGet("joined-campaigns")]
        public async Task<IActionResult> GetJoinedCampaigns()
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, data = " لا يوجد مستفيد" });


            var result = await _helpRequestService.GetJoinedCampaignsWithStatusAsync(userId);

            return Ok(new { success = true, data = result });
        }
      



        [Authorize]
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests([FromQuery] int pageNumber = 1,
 [FromQuery] int pageSize = 8)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, data = " لا يوجد مستفيد" });

            var result = await _helpRequestService.GetMyHelpRequestsAsync(userId, pageNumber, pageSize);

            // Return paginated shape similar to GetAllCampaign
            return Ok(new { success = true, data = result, pageNumber, pageSize });
        }


        #region Update & Delete item

        [Authorize]
        [HttpDelete("delete-my-request/{orderItemId}")]
        public async Task<IActionResult> DeleteMyRequest(int orderItemId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, message = "لا يوجد مستفيد" });

            var result = await _helpRequestService.DeleteMyRequestAsync(userId, orderItemId);

            if (!result.Success)
                return Ok(new { success = false, message = result.Message });

            return Ok(new { success = true, message = "تم حذف الطلب بنجاح" });
        }

        [Authorize]
        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity(int helpOrderId, int newQuantity)
        {
            var result = await _helpRequestService.UpdateHelpOrderQuantityAsync(helpOrderId, newQuantity);

            if (!result.Success)
                return Ok(new { success = false, message = result.Message });

            // لو مش محتاج ترجّع الـ UpdatedItem
            return Ok(new { success = true, message = result.Message });
        }

        #endregion


        [Authorize]
        [HttpPost("request-existing")]
        public async Task<IActionResult> RequestExistingProduct( [FromBody] ExistingProductRequestDto request)
        {
            if (request == null)   return Ok(new { success = false, data = " البيانات غير صحيحه" });
            // هتتظبط  بعد لما نضيف المصادقة

            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;


            var beneficiary = await _unitOfWork
     .Repository<Beneficiary, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return Ok(new { success = false, data = " لا يوجد مستفيد" });

            int beneficiaryId = beneficiary.Id;

            var result = await _helpRequestService.RequestExistingProductAsync(beneficiaryId, request);
            if (result.HelpOrderId == 0) return NotFound(new { success = false, message = result.Message });

            // return Arabic confirmation as requested
            return Ok(new { success = true, message = "تم استقبال طلبك", data = result });
        }

        // 2) Request a new product (name, condition, reason, image)
        // POST api/beneificery/{beneficiaryId}/request-new




       /* [Authorize]
        [HttpPost("request-new")]
        public async Task<IActionResult> RequestNewProduct( [FromForm] NewProductRequestDto request)
        {
            if (request == null) return Ok(new { success = false, data = " البيانات غير صحيحه" });
            // هتتظبط  بعد لما نضيف المصادقة

            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;


            var beneficiary = await _unitOfWork
     .Repository<Beneficiary, int>()
     .FirstOrDefaultAsync(b => b.AppUserId == userId);

            if (beneficiary == null)
                return Ok(new { success = false, data = " لا يوجد مستفيد" });

            int beneficiaryId = beneficiary.Id;

            var result = await _helpRequestService.RequestNewProductAsync(beneficiaryId, request);
            return Ok(new { success = true, data = result });
        }
*/



        [Authorize]
        [HttpGet("GetProfile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, data = " لا يوجد مستفيد" });

            var result = await _helpRequestService.GetProfileAsync(userId);
            return Ok(new { success = true, data = result });
        }

<<<<<<< HEAD



        [Authorize]
        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Ok(new { success = false, data = " لا يوجد مستفيد" });

            var success = await _helpRequestService.UpdateProfileAsync(userId, dto);

            return Ok(new { success, message = "Profile updated successfully" });
        }
=======
       
>>>>>>> solybranch
    }
}