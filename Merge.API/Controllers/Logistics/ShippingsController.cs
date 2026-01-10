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
using Merge.Application.Logistics.Commands.UpdateShippingStatus;
using Merge.Application.Order.Queries.GetOrderById;

namespace Merge.API.Controllers.Logistics;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/logistics/shippings")]
[Authorize]
public class ShippingsController : BaseController
{
    private readonly IMediator _mediator;

    public ShippingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Mevcut kargo sağlayıcılarını getirir
    /// </summary>
    [HttpGet("providers")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ShippingProviderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ShippingProviderDto>>> GetProviders(
        CancellationToken cancellationToken = default)
    {
        var query = new GetAvailableShippingProvidersQuery();
        var providers = await _mediator.Send(query, cancellationToken);
        return Ok(providers);
    }

    /// <summary>
    /// Kargo detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi siparişlerinin kargo bilgilerine erişebilir
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var query = new GetShippingByIdQuery(id);
        var shipping = await _mediator.Send(query, cancellationToken);
        if (shipping == null)
        {
            return NotFound();
        }

        // Order ownership kontrolü
        var orderQuery = new GetOrderByIdQuery(shipping.OrderId);
        var order = await _mediator.Send(orderQuery, cancellationToken);
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

    /// <summary>
    /// Siparişe ait kargo bilgilerini getirir
    /// </summary>
    [HttpGet("order/{orderId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> GetByOrderId(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi siparişlerinin kargo bilgilerine erişebilir
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var orderQuery = new GetOrderByIdQuery(orderId);
        var order = await _mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var query = new GetShippingByOrderIdQuery(orderId);
        var shipping = await _mediator.Send(query, cancellationToken);
        if (shipping == null)
        {
            return NotFound();
        }

        return Ok(shipping);
    }

    /// <summary>
    /// Kargo maliyetini hesaplar
    /// </summary>
    [HttpPost("calculate")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> CalculateCost(
        [FromBody] CalculateShippingDto dto,
        CancellationToken cancellationToken = default)
    {
        var query = new CalculateShippingCostQuery(dto.OrderId, dto.Provider);
        var cost = await _mediator.Send(query, cancellationToken);
        return Ok(new { cost });
    }

    /// <summary>
    /// Yeni kargo kaydı oluşturur (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> CreateShipping(
        [FromBody] CreateShippingDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateShippingCommand(dto.OrderId, dto.ShippingProvider, dto.ShippingCost);
        var shipping = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = shipping.Id }, shipping);
    }

    /// <summary>
    /// Kargo takip numarasını günceller (Admin only)
    /// </summary>
    [HttpPut("{shippingId}/tracking")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
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
        var command = new UpdateShippingTrackingCommand(shippingId, dto.TrackingNumber);
        var shipping = await _mediator.Send(command, cancellationToken);
        return Ok(shipping);
    }

    /// <summary>
    /// Kargo durumunu günceller (Admin only)
    /// </summary>
    [HttpPut("{shippingId}/status")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> UpdateStatus(
        Guid shippingId,
        [FromBody] UpdateShippingStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        if (!Enum.TryParse<ShippingStatus>(dto.Status, out var statusEnum))
        {
            return BadRequest("Geçersiz kargo durumu.");
        }

        var command = new UpdateShippingStatusCommand(shippingId, statusEnum);
        var shipping = await _mediator.Send(command, cancellationToken);
        return Ok(shipping);
    }
}

