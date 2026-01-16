using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Domain.Enums;
using Merge.API.Middleware;
using Merge.Application.Logistics.Queries.GetShippingById;
using Merge.Application.Logistics.Queries.GetShippingByOrderId;
using Merge.Application.Logistics.Queries.CalculateShippingCost;
using Merge.Application.Logistics.Queries.GetAvailableShippingProviders;
using Merge.Application.Logistics.Commands.CreateShipping;
using Merge.Application.Logistics.Commands.UpdateShippingTracking;
using Merge.Application.Logistics.Commands.PatchShippingTracking;
using Merge.Application.Logistics.Commands.UpdateShippingStatus;
using Merge.Application.Logistics.Commands.PatchShippingStatus;
using Merge.Application.Order.Queries.GetOrderById;

namespace Merge.API.Controllers.Logistics;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/logistics/shippings")]
[Authorize]
public class ShippingsController(IMediator mediator) : BaseController
{
    [HttpGet("providers")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ShippingProviderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ShippingProviderDto>>> GetProviders(
        CancellationToken cancellationToken = default)
    {
        var query = new GetAvailableShippingProvidersQuery();
        var providers = await mediator.Send(query, cancellationToken);
        return Ok(providers);
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var query = new GetShippingByIdQuery(id);
        var shipping = await mediator.Send(query, cancellationToken);
        if (shipping == null)
        {
            return NotFound();
        }

        // Order ownership kontrolü
        var orderQuery = new GetOrderByIdQuery(shipping.OrderId);
        var order = await mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(shipping);
    }

    [HttpGet("order/{orderId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> GetByOrderId(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var orderQuery = new GetOrderByIdQuery(orderId);
        var order = await mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var query = new GetShippingByOrderIdQuery(orderId);
        var shipping = await mediator.Send(query, cancellationToken);
        if (shipping == null)
        {
            return NotFound();
        }

        return Ok(shipping);
    }

    [HttpPost("calculate")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> CalculateCost(
        [FromBody] CalculateShippingDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var query = new CalculateShippingCostQuery(dto.OrderId, dto.Provider);
        var cost = await mediator.Send(query, cancellationToken);
        return Ok(new { cost });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> CreateShipping(
        [FromBody] CreateShippingDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new CreateShippingCommand(dto.OrderId, dto.ShippingProvider, dto.ShippingCost);
        var shipping = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = shipping.Id }, shipping);
    }

    [HttpPut("{shippingId}/tracking")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> UpdateTracking(
        Guid shippingId,
        [FromBody] UpdateTrackingDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new UpdateShippingTrackingCommand(shippingId, dto.TrackingNumber);
        var shipping = await mediator.Send(command, cancellationToken);
        return Ok(shipping);
    }

    [HttpPut("{shippingId}/status")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> UpdateStatus(
        Guid shippingId,
        [FromBody] UpdateShippingStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!Enum.TryParse<ShippingStatus>(dto.Status, out var statusEnum))
        {
            return BadRequest("Geçersiz kargo durumu.");
        }

        var command = new UpdateShippingStatusCommand(shippingId, statusEnum);
        var shipping = await mediator.Send(command, cancellationToken);
        return Ok(shipping);
    }

    /// <summary>
    /// Kargo takip numarasını kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{shippingId}/tracking")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> PatchTracking(
        Guid shippingId,
        [FromBody] PatchShippingTrackingDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new PatchShippingTrackingCommand(shippingId, patchDto);
        var shipping = await mediator.Send(command, cancellationToken);
        return Ok(shipping);
    }

    /// <summary>
    /// Kargo durumunu kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{shippingId}/status")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> PatchStatus(
        Guid shippingId,
        [FromBody] PatchShippingStatusDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new PatchShippingStatusCommand(shippingId, patchDto);
        var shipping = await mediator.Send(command, cancellationToken);
        return Ok(shipping);
    }
}
