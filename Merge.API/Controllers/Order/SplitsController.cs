using Microsoft.AspNetCore.Authorization;
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

    public OrderSplitsController(IOrderSplitService orderSplitService)
    {
        _orderSplitService = orderSplitService;
    }

    [HttpPost("order/{orderId}")]
    public async Task<ActionResult<OrderSplitDto>> SplitOrder(Guid orderId, [FromBody] CreateOrderSplitDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var split = await _orderSplitService.SplitOrderAsync(orderId, dto);
        return CreatedAtAction(nameof(GetSplit), new { id = split.Id }, split);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderSplitDto>> GetSplit(Guid id)
    {
        var split = await _orderSplitService.GetSplitAsync(id);
        if (split == null)
        {
            return NotFound();
        }
        return Ok(split);
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<IEnumerable<OrderSplitDto>>> GetOrderSplits(Guid orderId)
    {
        var splits = await _orderSplitService.GetOrderSplitsAsync(orderId);
        return Ok(splits);
    }

    [HttpGet("split-order/{splitOrderId}")]
    public async Task<ActionResult<IEnumerable<OrderSplitDto>>> GetSplitOrders(Guid splitOrderId)
    {
        var splits = await _orderSplitService.GetSplitOrdersAsync(splitOrderId);
        return Ok(splits);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelSplit(Guid id)
    {
        var success = await _orderSplitService.CancelSplitAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteSplit(Guid id)
    {
        var success = await _orderSplitService.CompleteSplitAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}

