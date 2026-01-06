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

namespace Merge.API.Controllers.Cart;

// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/cart/abandoned")]
public class AbandonedCartsController : BaseController
{
    private readonly IMediator _mediator;
    private readonly PaginationSettings _paginationSettings;

    public AbandonedCartsController(
        IMediator mediator,
        IOptions<PaginationSettings> paginationSettings)
    {
        _mediator = mediator;
        _paginationSettings = paginationSettings.Value;
    }

    /// <summary>
    /// Tüm terk edilmiş sepetleri getirir
    /// </summary>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Config'den al
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        var query = new GetAbandonedCartsQuery(minHours, maxDays, page, pageSize);
        var carts = await _mediator.Send(query, cancellationToken);
        return Ok(carts);
    }

    /// <summary>
    /// Terk edilmiş sepet detaylarını getirir
    /// </summary>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetAbandonedCartByIdQuery(cartId);
        var cart = await _mediator.Send(query, cancellationToken);

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
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(AbandonedCartRecoveryStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AbandonedCartRecoveryStatsDto>> GetRecoveryStats(
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetRecoveryStatsQuery(days);
        var stats = await _mediator.Send(query, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.1: FluentValidation - ValidationBehavior otomatik kontrol eder, manuel ValidateModelState() gereksiz
        var command = new SendRecoveryEmailCommand(
            cartId,
            dto.EmailType,
            dto.IncludeCoupon,
            dto.CouponDiscountPercentage);
        
        await _mediator.Send(command, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
        var command = new SendBulkRecoveryEmailsCommand(minHours, emailType);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// E-posta açılmasını takip eder (e-posta tracking pixel için)
    /// </summary>
    [HttpGet("track/open/{emailId}")]
    [AllowAnonymous]
    [RateLimit(100, 60)] // ✅ BOLUM 3.3: Rate Limiting - 100/dakika (spam koruması, AllowAnonymous)
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> TrackEmailOpen(
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new TrackEmailOpenCommand(emailId);
        await _mediator.Send(command, cancellationToken);

        // Return a 1x1 transparent pixel
        var pixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
        return File(pixel, "image/gif");
    }

    /// <summary>
    /// E-posta tıklamasını takip eder
    /// </summary>
    [HttpGet("track/click/{emailId}")]
    [AllowAnonymous]
    [RateLimit(100, 60)] // ✅ BOLUM 3.3: Rate Limiting - 100/dakika (spam koruması, AllowAnonymous)
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> TrackEmailClick(
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new TrackEmailClickCommand(emailId);
        await _mediator.Send(command, cancellationToken);
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
        var cartQuery = new GetAbandonedCartByIdQuery(cartId);
        var cart = await _mediator.Send(cartQuery, cancellationToken);
        
        if (cart == null)
        {
            return NotFound();
        }

        if (cart.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new MarkCartAsRecoveredCommand(cartId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Belirli bir sepet için e-posta geçmişini getirir
    /// </summary>
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Config'den al
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
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

            var cartQuery = new GetAbandonedCartByIdQuery(cartId);
            var cart = await _mediator.Send(cartQuery, cancellationToken);
            
            if (cart == null || cart.UserId != userId)
            {
                return Forbid();
            }
        }

        var query = new GetCartEmailHistoryQuery(cartId, page, pageSize);
        var history = await _mediator.Send(query, cancellationToken);
        return Ok(history);
    }
}
