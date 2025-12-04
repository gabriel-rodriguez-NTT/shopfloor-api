using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopfloorAssistant.AppService;
using System;
using System.Threading.Tasks;

namespace ShopfloorAssistant.Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PromptSuggestionsController : ControllerBase
    {
        private readonly IPromptSuggestionAppService _promptService;

        public PromptSuggestionsController(IPromptSuggestionAppService promptService)
        {
            _promptService = promptService;
        }

        // GET: api/PromptSuggestions
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _promptService.GetAllAsync();
            return Ok(items);
        }

        // GET: api/PromptSuggestions/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Parameter 'id' is not valid.");

            var item = await _promptService.GetByIdAsync(id);
            if (item == null) return NotFound();

            return Ok(item);
        }

        // POST: api/PromptSuggestions
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PromptSuggestionCreateDto dto)
        {
            if (dto == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(dto.Prompt)) return BadRequest("Prompt is required.");

            var created = await _promptService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/PromptSuggestions/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PromptSuggestionUpdateDto dto)
        {
            if (id == Guid.Empty) return BadRequest("Parameter 'id' is not valid.");
            if (dto == null) return BadRequest();

            try
            {
                await _promptService.UpdateAsync(id, dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/PromptSuggestions/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Parameter 'id' is not valid.");

            await _promptService.DeleteAsync(id);
            return NoContent();
        }
    }
}
