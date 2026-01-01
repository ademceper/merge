using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Support;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;


namespace Merge.API.Controllers.Support;

[ApiController]
[Route("api/support/faqs")]
public class FaqsController : BaseController
{
    private readonly IFaqService _faqService;
        public FaqsController(IFaqService faqService)
    {
        _faqService = faqService;
            }

    [HttpGet]
    public async Task<ActionResult<PagedResult<FaqDto>>> GetPublished(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var faqs = await _faqService.GetPublishedAsync(page, pageSize);
        return Ok(faqs);
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<PagedResult<FaqDto>>> GetByCategory(
        string category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var faqs = await _faqService.GetByCategoryAsync(category, page, pageSize);
        return Ok(faqs);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<FaqDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var faqs = await _faqService.GetAllAsync(page, pageSize);
        return Ok(faqs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FaqDto>> GetById(Guid id)
    {
        var faq = await _faqService.GetByIdAsync(id);
        if (faq == null)
        {
            return NotFound();
        }
        
        // Görüntülenme sayısını artır
        await _faqService.IncrementViewCountAsync(id);
        
        return Ok(faq);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FaqDto>> Create([FromBody] CreateFaqDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var faq = await _faqService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = faq.Id }, faq);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<FaqDto>> Update(Guid id, [FromBody] UpdateFaqDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var faq = await _faqService.UpdateAsync(id, dto);
        if (faq == null)
        {
            return NotFound();
        }
        return Ok(faq);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _faqService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

