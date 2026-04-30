using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KonSanad.APIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignService _service;

        public CampaignController(ICampaignService service)
        {
            _service = service;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
            {
                return NotFound();
            }

            return Ok(dto);
        }

    
        [HttpGet("GetAllCampiagn")]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var list = await _service.GetAllCampaignMAngerAsync(pageNumber, pageSize);
            return Ok(new { success = true, data = list });
        }
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CampaignCreateDto createDto)
        {
            var created = await _service.CreateAsync(createDto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [Authorize(Roles = "Manager")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CampaignCreateDto updateDto)
        {
            var ok = await _service.UpdateAsync(id, updateDto);
            if (!ok)
            {
                return NotFound();
            }

            return NoContent();
        }

        [Authorize(Roles = "Manager")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
