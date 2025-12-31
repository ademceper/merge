using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Logistics;


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

    [HttpGet("providers")]
    public async Task<ActionResult<IEnumerable<ShippingProviderDto>>> GetProviders()
    {
        var providers = await _shippingService.GetAvailableProvidersAsync();
        return Ok(providers);
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<ShippingDto>> GetByOrderId(Guid orderId)
    {
        // ✅ SECURITY: Authorization check - Users can only view shipping for their own orders or must be Admin/Manager
        var userId = GetUserId();
        var order = await _orderService.GetByIdAsync(orderId);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var shipping = await _shippingService.GetByOrderIdAsync(orderId);
        if (shipping == null)
        {
            return NotFound();
        }

        return Ok(shipping);
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<decimal>> CalculateCost([FromBody] CalculateShippingDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var cost = await _shippingService.CalculateShippingCostAsync(dto.OrderId, dto.Provider);
        return Ok(new { cost });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ShippingDto>> CreateShipping([FromBody] CreateShippingDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var shipping = await _shippingService.CreateShippingAsync(dto);
        return CreatedAtAction(nameof(GetByOrderId), new { orderId = shipping.OrderId }, shipping);
    }

    [HttpPut("{shippingId}/tracking")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ShippingDto>> UpdateTracking(Guid shippingId, [FromBody] UpdateTrackingDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var shipping = await _shippingService.UpdateTrackingAsync(shippingId, dto.TrackingNumber);
        if (shipping == null)
        {
            return NotFound();
        }
        return Ok(shipping);
    }

    [HttpPut("{shippingId}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ShippingDto>> UpdateStatus(Guid shippingId, [FromBody] UpdateShippingStatusDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var shipping = await _shippingService.UpdateStatusAsync(shippingId, dto.Status);
        if (shipping == null)
        {
            return NotFound();
        }
        return Ok(shipping);
    }
}

