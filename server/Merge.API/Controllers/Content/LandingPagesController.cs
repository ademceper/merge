using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Content.Commands.CreateLandingPage;
using Merge.Application.Content.Commands.UpdateLandingPage;
using Merge.Application.Content.Commands.DeleteLandingPage;
using Merge.Application.Content.Commands.PublishLandingPage;
using Merge.Application.Content.Commands.TrackLandingPageConversion;
using Merge.Application.Content.Commands.CreateLandingPageVariant;
using Merge.Application.Content.Queries.GetLandingPageById;
using Merge.Application.Content.Queries.GetLandingPageBySlug;
using Merge.Application.Content.Queries.GetAllLandingPages;
using Merge.Application.Content.Queries.GetLandingPageAnalytics;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Content;

/// <summary>
/// Landing Pages API endpoints.
/// Landing page'leri yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/content/landing-pages")]
[Tags("LandingPages")]
public class LandingPagesController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Yeni landing page oluşturur
    /// </summary>
    /// <param name="command">Landing page oluşturma komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan landing page</returns>
    /// <response code="201">Landing page başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(LandingPageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LandingPageDto>> CreateLandingPage(
        [FromBody] CreateLandingPageCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorId = GetUserId();
        var createCommand = command with { AuthorId = authorId };
        var landingPage = await mediator.Send(createCommand, cancellationToken);
        return CreatedAtAction(nameof(GetLandingPageById), new { id = landingPage.Id }, landingPage);
    }

    /// <summary>
    /// Landing page detaylarını getirir
    /// </summary>
    /// <param name="id">Landing page ID</param>
    /// <param name="trackView">View sayısını artır (varsayılan: false)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Landing page detayları</returns>
    /// <response code="200">Landing page başarıyla getirildi</response>
    /// <response code="404">Landing page bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(LandingPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LandingPageDto>> GetLandingPageById(
        Guid id,
        [FromQuery] bool trackView = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLandingPageByIdQuery(id, trackView);
        var landingPage = await mediator.Send(query, cancellationToken);
        if (landingPage == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(landingPage);
    }

    /// <summary>
    /// Slug'a göre landing page getirir
    /// </summary>
    /// <param name="slug">Landing page slug</param>
    /// <param name="trackView">View sayısını artır (varsayılan: true)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Landing page detayları</returns>
    /// <response code="200">Landing page başarıyla getirildi</response>
    /// <response code="404">Landing page bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(LandingPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LandingPageDto>> GetLandingPageBySlug(
        string slug,
        [FromQuery] bool trackView = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLandingPageBySlugQuery(slug, trackView);
        var landingPage = await mediator.Send(query, cancellationToken);
        if (landingPage == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(landingPage);
    }

    /// <summary>
    /// Tüm landing page'leri getirir (sayfalanmış)
    /// </summary>
    /// <param name="status">Durum filtresi (opsiyonel)</param>
    /// <param name="isActive">Aktif durum filtresi (opsiyonel)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış landing page listesi</returns>
    /// <response code="200">Landing page'ler başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<LandingPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<LandingPageDto>>> GetAllLandingPages(
        [FromQuery] string? status = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllLandingPagesQuery(status, isActive, page, pageSize);
        var landingPages = await mediator.Send(query, cancellationToken);
        return Ok(landingPages);
    }

    /// <summary>
    /// Landing page'i günceller
    /// </summary>
    /// <param name="id">Güncellenecek landing page ID</param>
    /// <param name="command">Landing page güncelleme komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Landing page başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Landing page bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateLandingPage(
        Guid id,
        [FromBody] UpdateLandingPageCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var updateCommand = command with { Id = id, PerformedBy = userId };
        var result = await mediator.Send(updateCommand, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    /// <summary>
    /// Landing page'i kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PatchLandingPage(
        Guid id,
        [FromBody] PatchLandingPageDto patchDto,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var performedBy = User.IsInRole("Admin") ? (Guid?)null : userId;
        var command = new UpdateLandingPageCommand(
            id,
            patchDto.Name,
            patchDto.Title,
            patchDto.Content,
            patchDto.Template,
            patchDto.Status,
            patchDto.StartDate,
            patchDto.EndDate,
            patchDto.MetaTitle,
            patchDto.MetaDescription,
            patchDto.OgImageUrl,
            patchDto.EnableABTesting,
            patchDto.TrafficSplit,
            performedBy);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    /// <summary>
    /// Landing page'i siler
    /// </summary>
    /// <param name="id">Silinecek landing page ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Landing page başarıyla silindi</response>
    /// <response code="404">Landing page bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteLandingPage(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new DeleteLandingPageCommand(id, userId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    /// <summary>
    /// Landing page'i yayınlar
    /// </summary>
    /// <param name="id">Yayınlanacak landing page ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Landing page başarıyla yayınlandı</response>
    /// <response code="404">Landing page bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PublishLandingPage(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new PublishLandingPageCommand(id, userId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    /// <summary>
    /// Landing page conversion'ı takip eder
    /// </summary>
    /// <param name="id">Conversion takip edilecek landing page ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Conversion başarıyla takip edildi</response>
    /// <response code="404">Landing page bulunamadı</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("{id}/track-conversion")]
    [AllowAnonymous]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (analytics için yüksek limit)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> TrackConversion(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new TrackLandingPageConversionCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    /// <summary>
    /// Landing page variant'ı oluşturur
    /// </summary>
    /// <param name="id">Variant oluşturulacak orijinal landing page ID</param>
    /// <param name="command">Variant oluşturma komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan variant</returns>
    /// <response code="201">Variant başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Orijinal landing page bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("{id}/create-variant")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(LandingPageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LandingPageDto>> CreateVariant(
        Guid id,
        [FromBody] CreateLandingPageVariantCommand command,
        CancellationToken cancellationToken = default)
    {
        var createCommand = command with { OriginalId = id };
        var variant = await mediator.Send(createCommand, cancellationToken);
        return CreatedAtAction(nameof(GetLandingPageById), new { id = variant.Id }, variant);
    }

    /// <summary>
    /// Landing page analytics'ini getirir
    /// </summary>
    /// <param name="id">Landing page ID</param>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Landing page analytics verileri</returns>
    /// <response code="200">Analytics başarıyla getirildi</response>
    /// <response code="404">Landing page bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("{id}/analytics")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika (analytics için yüksek limit)
    [ProducesResponseType(typeof(LandingPageAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LandingPageAnalyticsDto>> GetAnalytics(
        Guid id,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLandingPageAnalyticsQuery(id, startDate, endDate);
        var analytics = await mediator.Send(query, cancellationToken);
        return Ok(analytics);
    }
}

