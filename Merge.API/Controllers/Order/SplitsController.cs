using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Order;


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

    [HttpPost("order/{orderId}")]
    [ProducesResponseType(typeof(OrderSplitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderSplitDto>> SplitOrder(Guid orderId, [FromBody] CreateOrderSplitDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerini bölebilmeli
        var order = await _orderService.GetByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var split = await _orderSplitService.SplitOrderAsync(orderId, dto);
        return CreatedAtAction(nameof(GetSplit), new { id = split.Id }, split);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderSplitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderSplitDto>> GetSplit(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var split = await _orderSplitService.GetSplitAsync(id);
        if (split == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin split'lerine erişebilmeli
        var order = await _orderService.GetByIdAsync(split.OriginalOrderId);
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

    [HttpGet("order/{orderId}")]
    [ProducesResponseType(typeof(IEnumerable<OrderSplitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<OrderSplitDto>>> GetOrderSplits(Guid orderId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin split'lerine erişebilmeli
        var order = await _orderService.GetByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var splits = await _orderSplitService.GetOrderSplitsAsync(orderId);
        return Ok(splits);
    }

    [HttpGet("split-order/{splitOrderId}")]
    [ProducesResponseType(typeof(IEnumerable<OrderSplitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<OrderSplitDto>>> GetSplitOrders(Guid splitOrderId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin split'lerine erişebilmeli
        var splitOrder = await _orderService.GetByIdAsync(splitOrderId);
        if (splitOrder == null)
        {
            return NotFound();
        }

        if (splitOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var splits = await _orderSplitService.GetSplitOrdersAsync(splitOrderId);
        return Ok(splits);
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelSplit(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin split'lerini iptal edebilmeli
        var split = await _orderSplitService.GetSplitAsync(id);
        if (split == null)
        {
            return NotFound();
        }

        var order = await _orderService.GetByIdAsync(split.OriginalOrderId);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var success = await _orderSplitService.CancelSplitAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CompleteSplit(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Kullanıcı sadece kendi siparişlerinin split'lerini tamamlayabilmeli
        var split = await _orderSplitService.GetSplitAsync(id);
        if (split == null)
        {
            return NotFound();
        }

        var order = await _orderService.GetByIdAsync(split.OriginalOrderId);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var success = await _orderSplitService.CompleteSplitAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}

