using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Content;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;

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

    /// <summary>
    /// Yeni CMS sayfası oluşturur
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(CMSPageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CMSPageDto>> CreatePage([FromBody] CreateCMSPageDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var authorId = GetUserId();
        var page = await _cmsService.CreatePageAsync(authorId, dto);
        return CreatedAtAction(nameof(GetPageById), new { id = page.Id }, page);
    }

    /// <summary>
    /// CMS sayfası detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CMSPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CMSPageDto>> GetPageById(Guid id)
    {
        var page = await _cmsService.GetPageByIdAsync(id);
        if (page == null)
        {
            return NotFound();
        }
        return Ok(page);
    }

    /// <summary>
    /// Slug'a göre CMS sayfası getirir
    /// </summary>
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CMSPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CMSPageDto>> GetPageBySlug(string slug)
    {
        var page = await _cmsService.GetPageBySlugAsync(slug);
        if (page == null)
        {
            return NotFound();
        }
        return Ok(page);
    }

    /// <summary>
    /// Ana sayfa CMS içeriğini getirir
    /// </summary>
    [HttpGet("home")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CMSPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CMSPageDto>> GetHomePage()
    {
        var page = await _cmsService.GetHomePageAsync();
        if (page == null)
        {
            return NotFound();
        }
        return Ok(page);
    }

    /// <summary>
    /// Tüm CMS sayfalarını getirir (sayfalanmış)
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResult<CMSPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<CMSPageDto>>> GetAllPages(
        [FromQuery] string? status = null,
        [FromQuery] bool? showInMenu = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (pageSize > 100) pageSize = 100; // Max limit
        var pages = await _cmsService.GetAllPagesAsync(status, showInMenu, page, pageSize);
        return Ok(pages);
    }

    /// <summary>
    /// Menüde gösterilecek CMS sayfalarını getirir
    /// </summary>
    [HttpGet("menu")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CMSPageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CMSPageDto>>> GetMenuPages()
    {
        var pages = await _cmsService.GetMenuPagesAsync();
        return Ok(pages);
    }

    /// <summary>
    /// CMS sayfasını günceller
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
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

    /// <summary>
    /// CMS sayfasını siler
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePage(Guid id)
    {
        var result = await _cmsService.DeletePageAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// CMS sayfasını yayınlar
    /// </summary>
    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PublishPage(Guid id)
    {
        var result = await _cmsService.PublishPageAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// CMS sayfasını ana sayfa olarak ayarlar
    /// </summary>
    [HttpPost("{id}/set-home")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
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

