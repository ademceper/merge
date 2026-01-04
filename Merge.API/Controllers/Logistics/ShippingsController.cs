using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Logistics;
using Merge.Domain.Enums;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Logistics;

[ApiController]
[Route("api/logistics/shippings")]
[Authorize]
public class ShippingsController : BaseController
{
    private readonly IShippingService _shippingService;
    private readonly IOrderService _orderService;

    public ShippingsController(IShippingService shippingService, IOrderService orderService)
    {
        _shippingService = shippingService;
        _orderService = orderService;
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var providers = await _shippingService.GetAvailableProvidersAsync(cancellationToken);
        return Ok(providers);
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
        var shipping = await _shippingService.GetByOrderIdAsync(orderId, cancellationToken);
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
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var cost = await _shippingService.CalculateShippingCostAsync(dto.OrderId, dto.Provider, cancellationToken);
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
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var shipping = await _shippingService.CreateShippingAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetByOrderId), new { orderId = shipping.OrderId }, shipping);
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
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var shipping = await _shippingService.UpdateTrackingAsync(shippingId, dto.TrackingNumber, cancellationToken);
        if (shipping == null)
        {
            return NotFound();
        }
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
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        if (!Enum.TryParse<ShippingStatus>(dto.Status, out var statusEnum))
        {
            return BadRequest("Geçersiz kargo durumu.");
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var shipping = await _shippingService.UpdateStatusAsync(shippingId, statusEnum, cancellationToken);
        if (shipping == null)
        {
            return NotFound();
        }
        return Ok(shipping);
    }
}

