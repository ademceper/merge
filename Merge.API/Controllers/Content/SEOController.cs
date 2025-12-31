using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Content;
using Merge.Application.DTOs.Content;

namespace Merge.API.Controllers.Content;

[ApiController]
[Route("api/content/seo")]
[Authorize(Roles = "Admin,Manager")]
public class SEOController : BaseController
{
    private readonly ISEOService _seoService;

    public SEOController(ISEOService seoService)
    {
        _seoService = seoService;
    }

    [HttpPost("settings")]
    public async Task<ActionResult<SEOSettingsDto>> CreateOrUpdateSettings([FromBody] CreateSEOSettingsDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var settings = await _seoService.CreateOrUpdateSEOSettingsAsync(dto);
        return Ok(settings);
    }

    [HttpGet("settings")]
    [AllowAnonymous]
    public async Task<ActionResult<SEOSettingsDto>> GetSettings([FromQuery] string pageType, [FromQuery] Guid? entityId = null)
    {
        var settings = await _seoService.GetSEOSettingsAsync(pageType, entityId);
        if (settings == null)
        {
            return NotFound();
        }
        return Ok(settings);
    }

    [HttpDelete("settings")]
    public async Task<IActionResult> DeleteSettings([FromQuery] string pageType, [FromQuery] Guid entityId)
    {
        var success = await _seoService.DeleteSEOSettingsAsync(pageType, entityId);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("generate/product/{productId}")]
    public async Task<ActionResult<SEOSettingsDto>> GenerateProductSEO(Guid productId)
    {
        var settings = await _seoService.GenerateSEOForProductAsync(productId);
        return Ok(settings);
    }

    [HttpPost("generate/category/{categoryId}")]
    public async Task<ActionResult<SEOSettingsDto>> GenerateCategorySEO(Guid categoryId)
    {
        var settings = await _seoService.GenerateSEOForCategoryAsync(categoryId);
        return Ok(settings);
    }

    [HttpPost("generate/blog/{postId}")]
    public async Task<ActionResult<SEOSettingsDto>> GenerateBlogPostSEO(Guid postId)
    {
        var settings = await _seoService.GenerateSEOForBlogPostAsync(postId);
        return Ok(settings);
    }

    // Sitemap
    [HttpPost("sitemap/entries")]
    public async Task<ActionResult<SitemapEntryDto>> AddSitemapEntry([FromBody] AddSitemapEntryDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var entry = await _seoService.AddSitemapEntryAsync(
            dto.Url, 
            dto.PageType, 
            dto.EntityId, 
            dto.ChangeFrequency ?? "weekly", 
            dto.Priority ?? 0.5m);
        return CreatedAtAction(nameof(GetSitemapEntries), new { id = entry.Id }, entry);
    }

    [HttpGet("sitemap/entries")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<SitemapEntryDto>>> GetSitemapEntries([FromQuery] bool? isActive = null)
    {
        var entries = await _seoService.GetAllSitemapEntriesAsync(isActive);
        return Ok(entries);
    }

    [HttpPut("sitemap/entries/{id}")]
    public async Task<IActionResult> UpdateSitemapEntry(Guid id, [FromBody] UpdateSitemapEntryDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _seoService.UpdateSitemapEntryAsync(id, dto.Url, dto.ChangeFrequency, dto.Priority);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("sitemap/entries/{id}")]
    public async Task<IActionResult> RemoveSitemapEntry(Guid id)
    {
        var success = await _seoService.RemoveSitemapEntryAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("sitemap.xml")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSitemapXml()
    {
        var xml = await _seoService.GenerateSitemapXmlAsync();
        return Content(xml, "application/xml");
    }

    [HttpGet("robots.txt")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRobotsTxt()
    {
        var content = await _seoService.GenerateRobotsTxtAsync();
        return Content(content, "text/plain");
    }
}

