using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Application.DTOs.Content;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Content.Queries.GetBannerById;
using Merge.Application.Content.Queries.GetAllBanners;
using Merge.Application.Content.Queries.GetActiveBanners;
using Merge.Application.Content.Commands.CreateBanner;
using Merge.Application.Content.Commands.UpdateBanner;
using Merge.Application.Content.Commands.PatchBanner;
using Merge.Application.Content.Commands.DeleteBanner;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Content;

/// <summary>
/// Banners API endpoints.
/// Banner'ları yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/content/banners")]
[Tags("Banners")]
public class BannersController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Aktif banner'ları getirir
    /// </summary>
    /// <param name="position">Banner pozisyonu (opsiyonel)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış aktif banner listesi</returns>
    /// <response code="200">Aktif banner'lar başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<BannerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<BannerDto>>> GetActive(
        [FromQuery] string? position = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetActiveBannersQuery(position, page, pageSize);
        var banners = await mediator.Send(query, cancellationToken);
        return Ok(banners);
    }

    /// <summary>
    /// Tüm banner'ları getirir (Admin only)
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış banner listesi</returns>
    /// <response code="200">Banner'lar başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<BannerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<BannerDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetAllBannersQuery(page, pageSize);
        var banners = await mediator.Send(query, cancellationToken);
        return Ok(banners);
    }

    /// <summary>
    /// Banner detaylarını getirir
    /// </summary>
    /// <param name="id">Banner ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Banner detayları</returns>
    /// <response code="200">Banner başarıyla getirildi</response>
    /// <response code="404">Banner bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(BannerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BannerDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBannerByIdQuery(id);
        var banner = await mediator.Send(query, cancellationToken);
        
        if (banner == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(banner);
    }

    /// <summary>
    /// Yeni banner oluşturur
    /// </summary>
    /// <param name="command">Banner oluşturma komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan banner</returns>
    /// <response code="201">Banner başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(BannerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BannerDto>> Create(
        [FromBody] CreateBannerCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var banner = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = banner.Id }, banner);
    }

    /// <summary>
    /// Banner'ı günceller
    /// </summary>
    /// <param name="id">Banner ID</param>
    /// <param name="command">Banner güncelleme komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenen banner</returns>
    /// <response code="200">Banner başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">Banner bulunamadı</response>
    /// <response code="422">İş kuralı ihlali veya concurrency conflict</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(BannerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BannerDto>> Update(
        Guid id,
        [FromBody] UpdateBannerCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var updateCommand = command with { Id = id };
        var banner = await mediator.Send(updateCommand, cancellationToken);
        return Ok(banner);
    }

    /// <summary>
    /// Banner'ı kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(BannerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BannerDto>> Patch(
        Guid id,
        [FromBody] PatchBannerDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;
        var command = new PatchBannerCommand(id, patchDto);
        var banner = await mediator.Send(command, cancellationToken);
        return Ok(banner);
    }

    /// <summary>
    /// Banner'ı siler
    /// </summary>
    /// <param name="id">Banner ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Silme işlemi sonucu</returns>
    /// <response code="204">Banner başarıyla silindi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">Banner bulunamadı</response>
    /// <response code="422">İş kuralı ihlali veya concurrency conflict</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteBannerCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }
}

