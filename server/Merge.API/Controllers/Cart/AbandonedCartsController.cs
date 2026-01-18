using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Application.Cart.Queries.GetAbandonedCarts;
using Merge.Application.Cart.Queries.GetAbandonedCartById;
using Merge.Application.Cart.Queries.GetRecoveryStats;
using Merge.Application.Cart.Queries.GetCartEmailHistory;
using Merge.Application.Cart.Commands.SendRecoveryEmail;
using Merge.Application.Cart.Commands.SendBulkRecoveryEmails;
using Merge.Application.Cart.Commands.TrackEmailOpen;
using Merge.Application.Cart.Commands.TrackEmailClick;
using Merge.Application.Cart.Commands.MarkCartAsRecovered;
using Merge.Domain.Enums;
using Merge.API.Middleware;
using Merge.Application.Exceptions;

namespace Merge.API.Controllers.Cart;

/// <summary>
/// Abandoned Carts API endpoints.
/// Terk edilmiş sepetleri yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/cart/abandoned")]
[Tags("AbandonedCarts")]
public class AbandonedCartsController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Tüm terk edilmiş sepetleri getirir (Admin/Manager only)
    /// </summary>
    /// <param name="minHours">Minimum saat (varsayılan: 1)</param>
    /// <param name="maxDays">Maksimum gün (varsayılan: 30)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış terk edilmiş sepet listesi</returns>
    /// <response code="200">Terk edilmiş sepetler başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu işlem için Admin veya Manager yetkisi gerekli</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(PagedResult<AbandonedCartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<AbandonedCartDto>>> GetAbandonedCarts(
        [FromQuery] int minHours = 1,
        [FromQuery] int maxDays = 30,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetAbandonedCartsQuery(minHours, maxDays, page, pageSize);
        var carts = await mediator.Send(query, cancellationToken);
        return Ok(carts);
    }

    /// <summary>
    /// Terk edilmiş sepet detaylarını getirir (Admin/Manager only)
    /// </summary>
    /// <param name="cartId">Sepet ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Terk edilmiş sepet detayları</returns>
    /// <response code="200">Terk edilmiş sepet başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu işlem için Admin veya Manager yetkisi gerekli</response>
    /// <response code="404">Terk edilmiş sepet bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("{cartId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(AbandonedCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AbandonedCartDto>> GetAbandonedCartById(
        Guid cartId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAbandonedCartByIdQuery(cartId);
        var cart = await mediator.Send(query, cancellationToken)
            ?? throw new NotFoundException("AbandonedCart", cartId);

        return Ok(cart);
    }

    /// <summary>
    /// Terk edilmiş sepet kurtarma istatistiklerini getirir (Admin/Manager only)
    /// </summary>
    /// <param name="days">İstatistik için gün sayısı (varsayılan: 30)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kurtarma istatistikleri</returns>
    /// <response code="200">İstatistikler başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu işlem için Admin veya Manager yetkisi gerekli</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(AbandonedCartRecoveryStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AbandonedCartRecoveryStatsDto>> GetRecoveryStats(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRecoveryStatsQuery(days);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Belirli bir sepete kurtarma e-postası gönderir (Admin/Manager only)
    /// </summary>
    /// <param name="cartId">Sepet ID</param>
    /// <param name="dto">E-posta gönderme isteği</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">E-posta başarıyla gönderildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu işlem için Admin veya Manager yetkisi gerekli</response>
    /// <response code="404">Sepet bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("{cartId}/send-email")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 10 email / saat
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendRecoveryEmail(
        Guid cartId,
        [FromBody] SendRecoveryEmailDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new SendRecoveryEmailCommand(
            cartId,
            dto.EmailType,
            dto.IncludeCoupon,
            dto.CouponDiscountPercentage);
        
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Tüm uygun terk edilmiş sepetlere toplu kurtarma e-postası gönderir (Admin only)
    /// </summary>
    /// <param name="minHours">Minimum saat (varsayılan: 2)</param>
    /// <param name="emailType">E-posta tipi (varsayılan: First)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Toplu e-postalar başarıyla gönderildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu işlem için Admin yetkisi gerekli</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("send-bulk-emails")]
    [Authorize(Roles = "Admin")]
    [RateLimit(2, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 2 bulk email / saat
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendBulkRecoveryEmails(
        [FromQuery] int minHours = 2,
        [FromQuery] AbandonedCartEmailType emailType = AbandonedCartEmailType.First,
        CancellationToken cancellationToken = default)
    {
        var command = new SendBulkRecoveryEmailsCommand(minHours, emailType);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// E-posta açılmasını takip eder (e-posta tracking pixel için) - Anonymous erişim
    /// </summary>
    /// <param name="emailId">E-posta ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>1x1 transparent GIF pixel</returns>
    /// <response code="200">Tracking pixel başarıyla döndürüldü</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("track/open/{emailId}")]
    [AllowAnonymous]
    [RateLimit(100, 60)] // ✅ BOLUM 3.3: Rate Limiting - 100/dakika (spam koruması, AllowAnonymous)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> TrackEmailOpen(
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        var command = new TrackEmailOpenCommand(emailId);
        await mediator.Send(command, cancellationToken);

        // Return a 1x1 transparent pixel
        var pixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
        return File(pixel, "image/gif");
    }

    /// <summary>
    /// E-posta tıklamasını takip eder ve sepete yönlendirir - Anonymous erişim
    /// </summary>
    /// <param name="emailId">E-posta ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sepet sayfasına yönlendirme</returns>
    /// <response code="302">Sepet sayfasına yönlendirildi</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("track/click/{emailId}")]
    [AllowAnonymous]
    [RateLimit(100, 60)] // ✅ BOLUM 3.3: Rate Limiting - 100/dakika (spam koruması, AllowAnonymous)
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> TrackEmailClick(
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        var command = new TrackEmailClickCommand(emailId);
        await mediator.Send(command, cancellationToken);
        return Redirect("/cart"); // Redirect to cart page
    }

    /// <summary>
    /// Sepeti kurtarıldı olarak işaretler (kullanıcı satın alma tamamladığında çağrılır)
    /// </summary>
    /// <param name="cartId">Sepet ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Sepet başarıyla kurtarıldı olarak işaretlendi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu sepete erişim yetkisi yok</response>
    /// <response code="404">Sepet bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("{cartId}/mark-recovered")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkCartAsRecovered(
        Guid cartId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var cartQuery = new GetAbandonedCartByIdQuery(cartId);
        var cart = await mediator.Send(cartQuery, cancellationToken)
            ?? throw new NotFoundException("AbandonedCart", cartId);

        if (cart.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new MarkCartAsRecoveredCommand(cartId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Belirli bir sepet için e-posta geçmişini getirir (Admin/Manager veya sepet sahibi)
    /// </summary>
    /// <param name="cartId">Sepet ID</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış e-posta geçmişi</returns>
    /// <response code="200">E-posta geçmişi başarıyla getirildi</response>
    /// <response code="400">Geçersiz sayfalama parametreleri</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Bu sepete erişim yetkisi yok</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpGet("{cartId}/email-history")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(PagedResult<AbandonedCartEmailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<AbandonedCartEmailDto>>> GetCartEmailHistory(
        Guid cartId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        if (page < 1) page = 1;

        // Admin/Manager rolü varsa tüm cart email history'lerini görebilir
        // Normal kullanıcı sadece kendi cart'ının email history'sini görebilir
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var cartQuery = new GetAbandonedCartByIdQuery(cartId);
            var cart = await mediator.Send(cartQuery, cancellationToken);
            
            if (cart is null || cart.UserId != userId)
            {
                return Forbid();
            }
        }

        var query = new GetCartEmailHistoryQuery(cartId, page, pageSize);
        var history = await mediator.Send(query, cancellationToken);
        return Ok(history);
    }
}
