using AutoMapper;
using Azure.Core;
using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using Microsoft.AspNetCore.Mvc;
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

        public BeneificeryController(ICampaignService service, IMapper mapper, IHelpRequestService helpRequestService)
        {
            _service = service;
            _mapper = mapper;
            _helpRequestService = helpRequestService;
        }

        [HttpGet ("Campiagn")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }

        // 1) Request an existing donated product (only reason required)
        // POST api/beneificery/{beneficiaryId}/request-existing
        [HttpPost("{beneficiaryId:int}/request-existing")]
        public async Task<IActionResult> RequestExistingProduct(int beneficiaryId, [FromBody] ExistingProductRequestDto request)
        {
            if (request == null) return BadRequest(new { success = false, message = "Invalid request body" });

            var result = await _helpRequestService.RequestExistingProductAsync(beneficiaryId, request);
            if (result.HelpOrderId == 0) return NotFound(new { success = false, message = result.Message });

            return Ok(new { success = true, data = result });
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
