using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Application.Order.Commands.SplitOrder;
using Merge.Application.Order.Commands.CancelOrderSplit;
using Merge.Application.Order.Commands.CompleteOrderSplit;
using Merge.Application.Order.Queries.GetOrderSplit;
using Merge.Application.Order.Queries.GetOrderSplits;
using Merge.Application.Order.Queries.GetSplitOrders;
using Merge.Application.Order.Queries.GetOrderById;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Order;

/// <summary>
/// Order Split API endpoints.
/// Sipariş bölme işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/orders/splits")]
[Authorize(Roles = "Admin,Manager")]
[Tags("OrderSplits")]
public class OrderSplitsController(IMediator mediator) : BaseController
{
    [HttpPost("order/{orderId}")]
    [RateLimit(10, 60)]
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
        if (validationResult is not null) return validationResult;
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }
        var getOrderQuery = new GetOrderByIdQuery(orderId);
        var order = await mediator.Send(getOrderQuery, cancellationToken);
        if (order is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var command = new SplitOrderCommand(orderId, dto);
        var split = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetSplit), new { id = split.Id }, split);
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
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
        var getSplitQuery = new GetOrderSplitQuery(id);
        var split = await mediator.Send(getSplitQuery, cancellationToken);
        if (split is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        var getOrderQuery = new GetOrderByIdQuery(split.OriginalOrderId);
        var order = await mediator.Send(getOrderQuery, cancellationToken);
        if (order is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        return Ok(split);
    }

    [HttpGet("order/{orderId}")]
    [RateLimit(60, 60)]
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
        var getOrderQuery = new GetOrderByIdQuery(orderId);
        var order = await mediator.Send(getOrderQuery, cancellationToken);
        if (order is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var query = new GetOrderSplitsQuery(orderId);
        var splits = await mediator.Send(query, cancellationToken);
        return Ok(splits);
    }

    [HttpGet("split-order/{splitOrderId}")]
    [RateLimit(60, 60)]
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
        var getOrderQuery = new GetOrderByIdQuery(splitOrderId);
        var splitOrder = await mediator.Send(getOrderQuery, cancellationToken);
        if (splitOrder is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (splitOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var query = new GetSplitOrdersQuery(splitOrderId);
        var splits = await mediator.Send(query, cancellationToken);
        return Ok(splits);
    }

    [HttpPost("{id}/cancel")]
    [RateLimit(10, 60)]
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
        var getSplitQuery = new GetOrderSplitQuery(id);
        var split = await mediator.Send(getSplitQuery, cancellationToken);
        if (split is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        var getOrderQuery = new GetOrderByIdQuery(split.OriginalOrderId);
        var order = await mediator.Send(getOrderQuery, cancellationToken);
        if (order is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var command = new CancelOrderSplitCommand(id);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpPost("{id}/complete")]
    [RateLimit(10, 60)]
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
        var getSplitQuery = new GetOrderSplitQuery(id);
        var split = await mediator.Send(getSplitQuery, cancellationToken);
        if (split is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        var getOrderQuery = new GetOrderByIdQuery(split.OriginalOrderId);
        var order = await mediator.Send(getOrderQuery, cancellationToken);
        if (order is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var command = new CompleteOrderSplitCommand(id);
        var success = await mediator.Send(command, cancellationToken);
        if (!success)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }
}
