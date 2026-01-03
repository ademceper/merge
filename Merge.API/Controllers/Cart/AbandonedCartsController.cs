using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Cart;
using Merge.Application.DTOs.Cart;
using Merge.Application.Common;
using Merge.Domain.Enums;
using Merge.API.Middleware;


namespace Merge.API.Controllers.Cart;

[ApiController]
[Route("api/cart/abandoned")]
public class AbandonedCartsController : BaseController
{
    private readonly IAbandonedCartService _abandonedCartService;

    public AbandonedCartsController(IAbandonedCartService abandonedCartService)
    {
        _abandonedCartService = abandonedCartService;
    }

    /// <summary>
    /// Tüm terk edilmiş sepetleri getirir
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(MaxRequests = 30, WindowSeconds = 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
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
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var carts = await _abandonedCartService.GetAbandonedCartsAsync(minHours, maxDays, page, pageSize, cancellationToken);
        return Ok(carts);
    }

    /// <summary>
    /// Terk edilmiş sepet detaylarını getirir
    /// </summary>
    [HttpGet("{cartId}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(MaxRequests = 30, WindowSeconds = 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(AbandonedCartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AbandonedCartDto>> GetAbandonedCartById(
        Guid cartId,
        CancellationToken cancellationToken = default)
    {
        var cart = await _abandonedCartService.GetAbandonedCartByIdAsync(cartId, cancellationToken);

        if (cart == null)
        {
            return NotFound();
        }

        return Ok(cart);
    }

    /// <summary>
    /// Terk edilmiş sepet kurtarma istatistiklerini getirir
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(MaxRequests = 30, WindowSeconds = 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(AbandonedCartRecoveryStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AbandonedCartRecoveryStatsDto>> GetRecoveryStats(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var stats = await _abandonedCartService.GetRecoveryStatsAsync(days, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Belirli bir sepete kurtarma e-postası gönderir
    /// </summary>
    [HttpPost("{cartId}/send-email")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 10 email / saat
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendRecoveryEmail(
        Guid cartId,
        [FromBody] SendRecoveryEmailDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        await _abandonedCartService.SendRecoveryEmailAsync(
            cartId,
            dto.EmailType,
            dto.IncludeCoupon,
            dto.CouponDiscountPercentage,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Tüm uygun terk edilmiş sepetlere toplu kurtarma e-postası gönderir
    /// </summary>
    [HttpPost("send-bulk-emails")]
    [Authorize(Roles = "Admin")]
    [RateLimit(2, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 2 bulk email / saat
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SendBulkRecoveryEmails(
        [FromQuery] int minHours = 2,
        [FromQuery] AbandonedCartEmailType emailType = AbandonedCartEmailType.First,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
        await _abandonedCartService.SendBulkRecoveryEmailsAsync(minHours, emailType, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// E-posta açılmasını takip eder (e-posta tracking pixel için)
    /// </summary>
    [HttpGet("track/open/{emailId}")]
    [AllowAnonymous]
    [RateLimit(MaxRequests = 100, WindowSeconds = 60)] // ✅ BOLUM 3.3: Rate Limiting - 100/dakika (spam koruması, AllowAnonymous)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> TrackEmailOpen(
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        await _abandonedCartService.TrackEmailOpenAsync(emailId, cancellationToken);

        // Return a 1x1 transparent pixel
        var pixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
        return File(pixel, "image/gif");
    }

    /// <summary>
    /// E-posta tıklamasını takip eder
    /// </summary>
    [HttpGet("track/click/{emailId}")]
    [AllowAnonymous]
    [RateLimit(MaxRequests = 100, WindowSeconds = 60)] // ✅ BOLUM 3.3: Rate Limiting - 100/dakika (spam koruması, AllowAnonymous)
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> TrackEmailClick(
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        await _abandonedCartService.TrackEmailClickAsync(emailId, cancellationToken);
        return Redirect("/cart"); // Redirect to cart page
    }

    /// <summary>
    /// Sepeti kurtarıldı olarak işaretler (kullanıcı satın alma tamamladığında çağrılır)
    /// </summary>
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

        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        var cart = await _abandonedCartService.GetAbandonedCartByIdAsync(cartId, cancellationToken);
        if (cart == null)
        {
            return NotFound();
        }

        if (cart.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        await _abandonedCartService.MarkCartAsRecoveredAsync(cartId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Belirli bir sepet için e-posta geçmişini getirir
    /// </summary>
    [HttpGet("{cartId}/email-history")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(MaxRequests = 30, WindowSeconds = 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
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
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ BOLUM 3.2: IDOR Korumasi - Ownership check (ZORUNLU)
        // Admin/Manager rolü varsa tüm cart email history'lerini görebilir
        // Normal kullanıcı sadece kendi cart'ının email history'sini görebilir
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            if (!TryGetUserId(out var userId))
            {
                return Unauthorized();
            }

            var cart = await _abandonedCartService.GetAbandonedCartByIdAsync(cartId, cancellationToken);
            if (cart == null || cart.UserId != userId)
            {
                return Forbid();
            }
        }

        var history = await _abandonedCartService.GetCartEmailHistoryAsync(cartId, page, pageSize, cancellationToken);
        return Ok(history);
    }
}
