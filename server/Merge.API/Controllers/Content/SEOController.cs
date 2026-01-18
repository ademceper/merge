using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.API.Middleware;
using Merge.Application.Content.Commands.CreateOrUpdateSEOSettings;
using Merge.Application.Content.Commands.DeleteSEOSettings;
using Merge.Application.Content.Queries.GetSEOSettings;
using Merge.Application.Content.Commands.GenerateProductSEO;
using Merge.Application.Content.Commands.GenerateCategorySEO;
using Merge.Application.Content.Commands.GenerateBlogPostSEO;
using Merge.Application.Content.Commands.CreateSitemapEntry;
using Merge.Application.Content.Commands.UpdateSitemapEntry;
using Merge.Application.Content.Commands.DeleteSitemapEntry;
using Merge.Application.Content.Queries.GetSitemapEntries;
using Merge.Application.Content.Queries.GetSitemapXml;
using Merge.Application.Content.Queries.GetRobotsTxt;
using Merge.Application.Exceptions;

namespace Merge.API.Controllers.Content;

/// <summary>
/// SEO API endpoints.
/// SEO ayarlarını yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/content/seo")]
[Authorize(Roles = "Admin,Manager")]
[Tags("SEO")]
public class SEOController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// SEO ayarlarını oluşturur veya günceller
    /// </summary>
    /// <param name="command">SEO ayarları oluşturma/güncelleme komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan/güncellenen SEO ayarları</returns>
    /// <response code="200">SEO ayarları başarıyla oluşturuldu/güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("settings")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(SEOSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SEOSettingsDto>> CreateOrUpdateSettings(
        [FromBody] CreateOrUpdateSEOSettingsCommand command,
        CancellationToken cancellationToken = default)
    {
        var settings = await mediator.Send(command, cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// SEO ayarlarını getirir
    /// </summary>
    /// <param name="pageType">Sayfa tipi</param>
    /// <param name="entityId">Entity ID (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>SEO ayarları</returns>
    /// <response code="200">SEO ayarları başarıyla getirildi</response>
    /// <response code="404">SEO ayarları bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("settings")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(SEOSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SEOSettingsDto>> GetSettings(
        [FromQuery] string pageType,
        [FromQuery] Guid? entityId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSEOSettingsQuery(pageType, entityId);
        var settings = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("SEOSettings", pageType);

        return Ok(settings);
    }

    /// <summary>
    /// SEO ayarlarını siler
    /// </summary>
    /// <param name="pageType">Sayfa tipi</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">SEO ayarları başarıyla silindi</response>
    /// <response code="404">SEO ayarları bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("settings")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteSettings(
        [FromQuery] string pageType,
        [FromQuery] Guid entityId,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteSEOSettingsCommand(pageType, entityId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("SEOSettings", entityId);

        return NoContent();
    }

    /// <summary>
    /// Ürün için SEO ayarlarını otomatik oluşturur
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan SEO ayarları</returns>
    /// <response code="200">SEO ayarları başarıyla oluşturuldu</response>
    /// <response code="404">Ürün bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("generate/product/{productId}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(SEOSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SEOSettingsDto>> GenerateProductSEO(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var command = new GenerateProductSEOCommand(productId);
        var settings = await mediator.Send(command, cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Kategori için SEO ayarlarını otomatik oluşturur
    /// </summary>
    /// <param name="categoryId">Kategori ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan SEO ayarları</returns>
    /// <response code="200">SEO ayarları başarıyla oluşturuldu</response>
    /// <response code="404">Kategori bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("generate/category/{categoryId}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(SEOSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SEOSettingsDto>> GenerateCategorySEO(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var command = new GenerateCategorySEOCommand(categoryId);
        var settings = await mediator.Send(command, cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Blog post için SEO ayarlarını otomatik oluşturur
    /// </summary>
    /// <param name="postId">Blog post ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan SEO ayarları</returns>
    /// <response code="200">SEO ayarları başarıyla oluşturuldu</response>
    /// <response code="404">Blog post bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("generate/blog/{postId}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(SEOSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SEOSettingsDto>> GenerateBlogPostSEO(
        Guid postId,
        CancellationToken cancellationToken = default)
    {
        var command = new GenerateBlogPostSEOCommand(postId);
        var settings = await mediator.Send(command, cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Sitemap entry ekler
    /// </summary>
    /// <param name="command">Sitemap entry oluşturma komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan sitemap entry</returns>
    /// <response code="201">Sitemap entry başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("sitemap/entries")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(SitemapEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SitemapEntryDto>> AddSitemapEntry(
        [FromBody] CreateSitemapEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var entry = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetSitemapEntries), new { id = entry.Id }, entry);
    }

    /// <summary>
    /// Tüm sitemap entry'lerini getirir (sayfalanmış)
    /// </summary>
    /// <param name="isActive">Sadece aktif entry'leri getir (opsiyonel)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış sitemap entry listesi</returns>
    /// <response code="200">Sitemap entry'leri başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("sitemap/entries")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<SitemapEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<SitemapEntryDto>>> GetSitemapEntries(
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSitemapEntriesQuery(isActive, page, pageSize);
        var entries = await mediator.Send(query, cancellationToken);
        return Ok(entries);
    }

    /// <summary>
    /// Sitemap entry'yi günceller
    /// </summary>
    /// <param name="id">Güncellenecek sitemap entry ID</param>
    /// <param name="command">Sitemap entry güncelleme komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Sitemap entry başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Sitemap entry bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPut("sitemap/entries/{id}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateSitemapEntry(
        Guid id,
        [FromBody] UpdateSitemapEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var updateCommand = command with { Id = id };
        var result = await mediator.Send(updateCommand, cancellationToken);

        if (!result)
            throw new NotFoundException("SitemapEntry", id);

        return NoContent();
    }

    /// <summary>
    /// Sitemap entry'yi kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("sitemap/entries/{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PatchSitemapEntry(
        Guid id,
        [FromBody] PatchSitemapEntryDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        var command = new UpdateSitemapEntryCommand(
            id,
            patchDto.Url,
            patchDto.ChangeFrequency,
            patchDto.Priority);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("SitemapEntry", id);

        return NoContent();
    }

    /// <summary>
    /// Sitemap entry'yi siler
    /// </summary>
    /// <param name="id">Silinecek sitemap entry ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Sitemap entry başarıyla silindi</response>
    /// <response code="404">Sitemap entry bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("sitemap/entries/{id}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RemoveSitemapEntry(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteSitemapEntryCommand(id);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("SitemapEntry", id);

        return NoContent();
    }

    /// <summary>
    /// Sitemap XML'ini getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sitemap XML içeriği</returns>
    /// <response code="200">Sitemap XML başarıyla getirildi</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("sitemap.xml")]
    [AllowAnonymous]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (sitemap için düşük limit)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetSitemapXml(
        CancellationToken cancellationToken = default)
    {
        var query = new GetSitemapXmlQuery();
        var xml = await mediator.Send(query, cancellationToken);
        return Content(xml, "application/xml");
    }

    /// <summary>
    /// Robots.txt içeriğini getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Robots.txt içeriği</returns>
    /// <response code="200">Robots.txt başarıyla getirildi</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("robots.txt")]
    [AllowAnonymous]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (robots.txt için düşük limit)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetRobotsTxt(
        CancellationToken cancellationToken = default)
    {
        var query = new GetRobotsTxtQuery();
        var content = await mediator.Send(query, cancellationToken);
        return Content(content, "text/plain");
    }
}

