using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Order;
using Merge.API.Middleware;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
namespace Merge.API.Controllers.Order;

[ApiController]
[Route("api/orders/splits")]
[Authorize(Roles = "Admin,Manager")]
public class OrderSplitsController : BaseController
{
    private readonly IOrderSplitService _orderSplitService;
    private readonly IOrderService _orderService;

    public OrderSplitsController(IOrderSplitService orderSplitService, IOrderService orderService)
    {
        _orderSplitService = orderSplitService;
        _orderService = orderService;
    }

    /// <summary>
    /// Siparişi böler
    /// </summary>
    [HttpPost("order/{orderId}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(typeof(OrderSplitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderSplitDto>> SplitOrder(
        Guid orderId,
        [FromBody] CreateOrderSplitDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerini bölebilmeli
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var split = await _orderSplitService.SplitOrderAsync(orderId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetSplit), new { id = split.Id }, split);
    }

    /// <summary>
    /// Split detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(OrderSplitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderSplitDto>> GetSplit(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var split = await _orderSplitService.GetSplitAsync(id, cancellationToken);
        if (split == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin split'lerine erişebilmeli
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var order = await _orderService.GetByIdAsync(split.OriginalOrderId, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(split);
    }

    /// <summary>
    /// Siparişin split'lerini getirir
    /// </summary>
    [HttpGet("order/{orderId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<OrderSplitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<OrderSplitDto>>> GetOrderSplits(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin split'lerine erişebilmeli
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var splits = await _orderSplitService.GetOrderSplitsAsync(orderId, cancellationToken);
        return Ok(splits);
    }

    /// <summary>
    /// Split order'ın split'lerini getirir
    /// </summary>
    [HttpGet("split-order/{splitOrderId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<OrderSplitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<OrderSplitDto>>> GetSplitOrders(
        Guid splitOrderId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin split'lerine erişebilmeli
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var splitOrder = await _orderService.GetByIdAsync(splitOrderId, cancellationToken);
        if (splitOrder == null)
        {
            return NotFound();
        }

        if (splitOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var splits = await _orderSplitService.GetSplitOrdersAsync(splitOrderId, cancellationToken);
        return Ok(splits);
    }

    /// <summary>
    /// Split'i iptal eder
    /// </summary>
    [HttpPost("{id}/cancel")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CancelSplit(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin split'lerini iptal edebilmeli
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var split = await _orderSplitService.GetSplitAsync(id, cancellationToken);
        if (split == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var order = await _orderService.GetByIdAsync(split.OriginalOrderId, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _orderSplitService.CancelSplitAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Split'i tamamlar
    /// </summary>
    [HttpPost("{id}/complete")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CompleteSplit(Guid id, CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin split'lerini tamamlayabilmeli
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var split = await _orderSplitService.GetSplitAsync(id, cancellationToken);
        if (split == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var order = await _orderService.GetByIdAsync(split.OriginalOrderId, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _orderSplitService.CompleteSplitAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}

