using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Content.Commands.CreatePageBuilder;
using Merge.Application.Content.Commands.UpdatePageBuilder;
using Merge.Application.Content.Commands.DeletePageBuilder;
using Merge.Application.Content.Commands.PublishPageBuilder;
using Merge.Application.Content.Commands.UnpublishPageBuilder;
using Merge.Application.Content.Queries.GetPageBuilderById;
using Merge.Application.Content.Queries.GetPageBuilderBySlug;
using Merge.Application.Content.Queries.GetAllPageBuilders;
using Merge.API.Middleware;
using Merge.Application.Exceptions;

namespace Merge.API.Controllers.Content;

/// <summary>
/// Page Builders API endpoints.
/// Sayfa builder işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/content/page-builders")]
[Authorize]
[Tags("PageBuilders")]
public class PageBuildersController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Yeni page builder sayfası oluşturur
    /// </summary>
    /// <param name="command">Page builder oluşturma komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan page builder</returns>
    /// <response code="201">Page builder başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(PageBuilderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PageBuilderDto>> CreatePage(
        [FromBody] CreatePageBuilderCommand command,
        CancellationToken cancellationToken = default)
    {
        var authorId = GetUserId();
        var createCommand = command with { AuthorId = authorId };
        var page = await mediator.Send(createCommand, cancellationToken);
        return CreatedAtAction(nameof(GetPage), new { id = page.Id }, page);
    }

    /// <summary>
    /// Page builder sayfası detaylarını getirir
    /// </summary>
    /// <param name="id">Page builder ID</param>
    /// <param name="trackView">View sayısını artır (varsayılan: false)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Page builder detayları</returns>
    /// <response code="200">Page builder başarıyla getirildi</response>
    /// <response code="404">Page builder bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PageBuilderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PageBuilderDto>> GetPage(
        Guid id,
        [FromQuery] bool trackView = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPageBuilderByIdQuery(id, trackView);
        var page = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("PageBuilder", id);

        return Ok(page);
    }

    /// <summary>
    /// Slug'a göre page builder sayfası getirir
    /// </summary>
    /// <param name="slug">Page builder slug</param>
    /// <param name="trackView">View sayısını artır (varsayılan: true)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Page builder detayları</returns>
    /// <response code="200">Page builder başarıyla getirildi</response>
    /// <response code="404">Page builder bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("slug/{slug}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PageBuilderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PageBuilderDto>> GetPageBySlug(
        string slug,
        [FromQuery] bool trackView = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPageBuilderBySlugQuery(slug, trackView);
        var page = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("PageBuilder", slug);

        return Ok(page);
    }

    /// <summary>
    /// Tüm page builder sayfalarını getirir (sayfalanmış)
    /// </summary>
    /// <param name="status">Durum filtresi (opsiyonel)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış page builder listesi</returns>
    /// <response code="200">Page builder'lar başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<PageBuilderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PageBuilderDto>>> GetAllPages(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllPageBuildersQuery(status, page, pageSize);
        var pages = await mediator.Send(query, cancellationToken);
        return Ok(pages);
    }

    /// <summary>
    /// Page builder sayfasını günceller
    /// </summary>
    /// <param name="id">Güncellenecek page builder ID</param>
    /// <param name="command">Page builder güncelleme komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Page builder başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="404">Page builder bulunamadı</response>
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
    public async Task<IActionResult> UpdatePage(
        Guid id,
        [FromBody] UpdatePageBuilderCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var updateCommand = command with { Id = id, PerformedBy = userId };
        var result = await mediator.Send(updateCommand, cancellationToken);

        if (!result)
            throw new NotFoundException("PageBuilder", id);

        return NoContent();
    }

    /// <summary>
    /// Page builder sayfasını kısmi olarak günceller (PATCH)
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
    public async Task<IActionResult> PatchPage(
        Guid id,
        [FromBody] PatchPageBuilderDto patchDto,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var performedBy = User.IsInRole("Admin") ? (Guid?)null : userId;
        var command = new UpdatePageBuilderCommand(
            id,
            patchDto.Slug,
            patchDto.Name,
            patchDto.Title,
            patchDto.Content,
            patchDto.Template,
            patchDto.PageType,
            patchDto.RelatedEntityId,
            patchDto.MetaTitle,
            patchDto.MetaDescription,
            patchDto.OgImageUrl,
            performedBy);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("PageBuilder", id);

        return NoContent();
    }

    /// <summary>
    /// Page builder sayfasını siler
    /// </summary>
    /// <param name="id">Silinecek page builder ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Page builder başarıyla silindi</response>
    /// <response code="404">Page builder bulunamadı</response>
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
    public async Task<IActionResult> DeletePage(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new DeletePageBuilderCommand(id, userId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("PageBuilder", id);

        return NoContent();
    }

    /// <summary>
    /// Page builder sayfasını yayınlar
    /// </summary>
    /// <param name="id">Yayınlanacak page builder ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Page builder başarıyla yayınlandı</response>
    /// <response code="404">Page builder bulunamadı</response>
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
    public async Task<IActionResult> PublishPage(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new PublishPageBuilderCommand(id, userId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("PageBuilder", id);

        return NoContent();
    }

    /// <summary>
    /// Page builder sayfasını yayından kaldırır
    /// </summary>
    /// <param name="id">Yayından kaldırılacak page builder ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Page builder başarıyla yayından kaldırıldı</response>
    /// <response code="404">Page builder bulunamadı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("{id}/unpublish")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)] // BusinessException için
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UnpublishPage(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new UnpublishPageBuilderCommand(id, userId);
        var result = await mediator.Send(command, cancellationToken);

        if (!result)
            throw new NotFoundException("PageBuilder", id);

        return NoContent();
    }
}
