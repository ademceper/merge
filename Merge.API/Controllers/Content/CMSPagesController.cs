using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Content;
using Merge.Application.DTOs.Content;

namespace Merge.API.Controllers.Content;

[ApiController]
[Route("api/content/cms-pages")]
public class CMSPagesController : BaseController
{
    private readonly ICMSService _cmsService;

    public CMSPagesController(ICMSService cmsService)
    {
        _cmsService = cmsService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CMSPageDto>> CreatePage([FromBody] CreateCMSPageDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var authorId = GetUserId();
        var page = await _cmsService.CreatePageAsync(authorId, dto);
        return CreatedAtAction(nameof(GetPageById), new { id = page.Id }, page);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CMSPageDto>> GetPageById(Guid id)
    {
        var page = await _cmsService.GetPageByIdAsync(id);
        if (page == null)
        {
            return NotFound();
        }
        return Ok(page);
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<CMSPageDto>> GetPageBySlug(string slug)
    {
        var page = await _cmsService.GetPageBySlugAsync(slug);
        if (page == null)
        {
            return NotFound();
        }
        return Ok(page);
    }

    [HttpGet("home")]
    [AllowAnonymous]
    public async Task<ActionResult<CMSPageDto>> GetHomePage()
    {
        var page = await _cmsService.GetHomePageAsync();
        if (page == null)
        {
            return NotFound();
        }
        return Ok(page);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CMSPageDto>>> GetAllPages([FromQuery] string? status = null, [FromQuery] bool? showInMenu = null)
    {
        var pages = await _cmsService.GetAllPagesAsync(status, showInMenu);
        return Ok(pages);
    }

    [HttpGet("menu")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CMSPageDto>>> GetMenuPages()
    {
        var pages = await _cmsService.GetMenuPagesAsync();
        return Ok(pages);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdatePage(Guid id, [FromBody] CreateCMSPageDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _cmsService.UpdatePageAsync(id, dto);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeletePage(Guid id)
    {
        var result = await _cmsService.DeletePageAsync(id);
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
        var result = await _cmsService.PublishPageAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/set-home")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> SetHomePage(Guid id)
    {
        var result = await _cmsService.SetHomePageAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}

