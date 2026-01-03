using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Content;
using Merge.Application.DTOs.Content;
using Merge.API.Middleware;

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

    /// <summary>
    /// SEO ayarlarını oluşturur veya günceller
    /// </summary>
    [HttpPost("settings")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(SEOSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SEOSettingsDto>> CreateOrUpdateSettings(
        [FromBody] CreateSEOSettingsDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var settings = await _seoService.CreateOrUpdateSEOSettingsAsync(dto, cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// SEO ayarlarını getirir
    /// </summary>
    [HttpGet("settings")]
    [AllowAnonymous]
    [RateLimit(MaxRequests = 60, WindowSeconds = 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(SEOSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SEOSettingsDto>> GetSettings(
        [FromQuery] string pageType,
        [FromQuery] Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var settings = await _seoService.GetSEOSettingsAsync(pageType, entityId, cancellationToken);
        if (settings == null)
        {
            return NotFound();
        }
        return Ok(settings);
    }

    /// <summary>
    /// SEO ayarlarını siler
    /// </summary>
    [HttpDelete("settings")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteSettings(
        [FromQuery] string pageType,
        [FromQuery] Guid entityId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _seoService.DeleteSEOSettingsAsync(pageType, entityId, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Ürün için SEO ayarlarını otomatik oluşturur
    /// </summary>
    [HttpPost("generate/product/{productId}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(SEOSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SEOSettingsDto>> GenerateProductSEO(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var settings = await _seoService.GenerateSEOForProductAsync(productId, cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Kategori için SEO ayarlarını otomatik oluşturur
    /// </summary>
    [HttpPost("generate/category/{categoryId}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(SEOSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SEOSettingsDto>> GenerateCategorySEO(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var settings = await _seoService.GenerateSEOForCategoryAsync(categoryId, cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Blog post için SEO ayarlarını otomatik oluşturur
    /// </summary>
    [HttpPost("generate/blog/{postId}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(SEOSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SEOSettingsDto>> GenerateBlogPostSEO(
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var settings = await _seoService.GenerateSEOForBlogPostAsync(postId, cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Sitemap entry ekler
    /// </summary>
    [HttpPost("sitemap/entries")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(SitemapEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SitemapEntryDto>> AddSitemapEntry(
        [FromBody] AddSitemapEntryDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var entry = await _seoService.AddSitemapEntryAsync(
            dto.Url, 
            dto.PageType, 
            dto.EntityId, 
            dto.ChangeFrequency ?? "weekly", 
            dto.Priority ?? 0.5m,
            cancellationToken);
        return CreatedAtAction(nameof(GetSitemapEntries), new { id = entry.Id }, entry);
    }

    /// <summary>
    /// Tüm sitemap entry'lerini getirir (sayfalanmış)
    /// </summary>
    [HttpGet("sitemap/entries")]
    [AllowAnonymous]
    [RateLimit(MaxRequests = 60, WindowSeconds = 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<SitemapEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<SitemapEntryDto>>> GetSitemapEntries(
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ⚠️ NOT: GetAllSitemapEntriesAsync pagination desteklemiyor - Interface'i güncellemek gerekiyor
        var entries = await _seoService.GetAllSitemapEntriesAsync(isActive, page, pageSize, cancellationToken);
        return Ok(entries);
    }

    /// <summary>
    /// Sitemap entry'yi günceller
    /// </summary>
    [HttpPut("sitemap/entries/{id}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateSitemapEntry(
        Guid id,
        [FromBody] UpdateSitemapEntryDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _seoService.UpdateSitemapEntryAsync(id, dto.Url, dto.ChangeFrequency, dto.Priority, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Sitemap entry'yi siler
    /// </summary>
    [HttpDelete("sitemap/entries/{id}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveSitemapEntry(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _seoService.RemoveSitemapEntryAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Sitemap XML'ini getirir
    /// </summary>
    [HttpGet("sitemap.xml")]
    [AllowAnonymous]
    [RateLimit(MaxRequests = 10, WindowSeconds = 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (sitemap için düşük limit)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetSitemapXml(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var xml = await _seoService.GenerateSitemapXmlAsync(cancellationToken);
        return Content(xml, "application/xml");
    }

    /// <summary>
    /// Robots.txt içeriğini getirir
    /// </summary>
    [HttpGet("robots.txt")]
    [AllowAnonymous]
    [RateLimit(MaxRequests = 10, WindowSeconds = 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (robots.txt için düşük limit)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetRobotsTxt(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var content = await _seoService.GenerateRobotsTxtAsync(cancellationToken);
        return Content(content, "text/plain");
    }
}

