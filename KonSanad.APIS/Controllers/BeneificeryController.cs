using AutoMapper;
using Azure.Core;
using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using KonSanad.Repository.Entities;
using KonSanad.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        public BeneificeryController(ICampaignService service, IMapper mapper, IDonationService ORder, IHelpRequestService helpRequestService)
        {
            _service = service;
            _mapper = mapper;
            _helpRequestService = helpRequestService;
            _ORder = ORder;
        }

        [Authorize]
        [HttpGet("GetAllCampiagn")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(new { success = true, data = list } );
        }

        [HttpGet("GetCampaignById")]
        public async Task<ActionResult<CampaignDto>> GetById(int id)
        {
            var campaignDto = await _service.GetByIdAsync(id);

            if (campaignDto == null)
                return NotFound(new ApiResponse(404, $"الحملة رقم {id} غير موجودة"));

            return Ok(campaignDto);
        }

        [HttpPost("join-campaign")]
        public async Task<IActionResult> JoinCampaign(int id, [FromBody] JoinCampaignRequestDto request)
        {

          //  var userId = User.FindFirst("uid")?.Value;
           
         //   Console.WriteLine($"User ID from token: {userId}  , type {userId.GetType()}");
            try
            {
                var message = await _helpRequestService.JoinCampaignAsync(id, request);
                return Ok(new { success = true, data = message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("order-item")]
        public async Task<IActionResult> GetOrderItem()
        {
            var orderItem = await _ORder.GetAllDonationItemsAsync();

            if (orderItem == null)
                return NotFound(new { success = false, message = "الصنف غير موجود" });

            //var dto = _mapper.Map<OrderItemDto>(orderItem);

            return Ok(new { success = true, data = orderItem });
        }

        [HttpGet("order-item/{id:int}")]
        public async Task<IActionResult> GetOrderItemById(int id)
        {
            var item = await _ORder.GetOrderItemByIdAsync(id);

            if (item == null)
                return NotFound(new { success = false, message = $"الصنف رقم {id} غير موجود أو تم حذفه" });

            return Ok(new { success = true, data = item });
        }
        // 1) Request an existing donated product (only reason required)
        // POST api/beneificery/{beneficiaryId}/request-existing
        [HttpPost("request-existing")]
        public async Task<IActionResult> RequestExistingProduct(int id, [FromBody] ExistingProductRequestDto request)
        {
            if (request == null) return BadRequest(new { success = false, message = "Invalid request body" });
            // هتتظبط  بعد لما نضيف المصادقة

            var userId = User.FindFirst("uid")?.Value;

            var result = await _helpRequestService.RequestExistingProductAsync( id, request);
            if (result.HelpOrderId == 0) return NotFound(new { success = false, message = result.Message });

            // return Arabic confirmation as requested
            return Ok(new { success = true, message = "تم استقبال طلبك", data = result });
        }

        // 2) Request a new product (name, condition, reason, image)
        // POST api/beneificery/{beneficiaryId}/request-new
        [HttpPost("request-new")]
        public async Task<IActionResult> RequestNewProduct(int id, [FromForm] NewProductRequestDto request)
        {
            if (request == null) return BadRequest(new { success = false, message = "Invalid request body" });
            // هتتظبط  بعد لما نضيف المصادقة

            var userId = User.FindFirst("uid")?.Value;

            var result = await _helpRequestService.RequestNewProductAsync(id, request);
            return Ok(new { success = true, data = result });
        }
    }
}