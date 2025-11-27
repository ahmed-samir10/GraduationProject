using AutoMapper;
using KonSanad.Core.Dtos;
using KonSanad.Core.IRepositery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace KonSanad.APIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignService _service;
        private readonly IMapper _mapper;

        public CampaignsController(ICampaignService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

       

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CampaignCreateDto createDto)
        {
            var created = await _service.CreateAsync(createDto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CampaignCreateDto updateDto)
        {
            var ok = await _service.UpdateAsync(id, updateDto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}