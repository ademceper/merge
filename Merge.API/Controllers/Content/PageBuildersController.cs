using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Services;
using Merge.Application.Interfaces.Content;
using Merge.Application.DTOs.Content;


namespace Merge.API.Controllers.Content;

[ApiController]
[Route("api/content/page-builders")]
[Authorize]
public class PageBuildersController : BaseController
{
    private readonly IPageBuilderService _pageBuilderService;
        public PageBuildersController(IPageBuilderService pageBuilderService)
    {
        _pageBuilderService = pageBuilderService;
            }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<PageBuilderDto>> CreatePage([FromBody] CreatePageBuilderDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var page = await _pageBuilderService.CreatePageAsync(dto);
        return CreatedAtAction(nameof(GetPage), new { id = page.Id }, page);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PageBuilderDto>> GetPage(Guid id)
    {
        var page = await _pageBuilderService.GetPageAsync(id);
        if (page == null)
        {
            return NotFound();
        }
        return Ok(page);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<PageBuilderDto>> GetPageBySlug(string slug)
    {
        var page = await _pageBuilderService.GetPageBySlugAsync(slug);
        if (page == null)
        {
            return NotFound();
        }
        return Ok(page);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<PageBuilderDto>>> GetAllPages([FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var pages = await _pageBuilderService.GetAllPagesAsync(status, page, pageSize);
        return Ok(pages);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdatePage(Guid id, [FromBody] UpdatePageBuilderDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _pageBuilderService.UpdatePageAsync(id, dto);
        if (result == false)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeletePage(Guid id)
    {
        var result = await _pageBuilderService.DeletePageAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> PublishPage(Guid id)
    {
        var result = await _pageBuilderService.PublishPageAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/unpublish")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UnpublishPage(Guid id)
    {
        var result = await _pageBuilderService.UnpublishPageAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

