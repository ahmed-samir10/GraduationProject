using AutoMapper;
using Azure.Core;
using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using KonSanad.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KonSanad.APIS.Controllers
{
    // [Authorize (Roles = "Beneficiary")]
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

        [HttpGet("GetAllCampiagn")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("GetCampaignById")]
        public async Task<ActionResult<CampaignDto>> GetById(int id)
        {
            var campaignDto = await _service.GetByIdAsync(id);

            if (campaignDto == null)
                return NotFound(new ApiResponse(404, $"الحملة رقم {id} غير موجودة"));

            return Ok(campaignDto);
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
        [HttpPost("{beneficiaryId:int}/request-existing")]
        public async Task<IActionResult> RequestExistingProduct(int beneficiaryId, [FromBody] ExistingProductRequestDto request)
        {
            if (request == null) return BadRequest(new { success = false, message = "Invalid request body" });

            var result = await _helpRequestService.RequestExistingProductAsync(beneficiaryId, request);
            if (result.HelpOrderId == 0) return NotFound(new { success = false, message = result.Message });

            // return Arabic confirmation as requested
            return Ok(new { success = true, message = "تم استقبال طلبك", data = result });
        }

        // 2) Request a new product (name, condition, reason, image)
        // POST api/beneificery/{beneficiaryId}/request-new
        [HttpPost("{beneficiaryId:int}/request-new")]
        public async Task<IActionResult> RequestNewProduct(int beneficiaryId, [FromBody] NewProductRequestDto request)
        {
            if (request == null) return BadRequest(new { success = false, message = "Invalid request body" });

            var result = await _helpRequestService.RequestNewProductAsync(beneficiaryId, request);
            return Ok(new { success = true, data = result });
        }
    }
}