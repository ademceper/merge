using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;
using Merge.Domain.Enums;


namespace Merge.API.Controllers.Order;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : BaseController
{
    private readonly IOrderService _orderService;
    private readonly IOrderFilterService _orderFilterService;
        public OrdersController(
        IOrderService orderService,
        IOrderFilterService orderFilterService)
    {
        _orderService = orderService;
        _orderFilterService = orderFilterService;
            }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetMyOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var orders = await _orderService.GetOrdersByUserIdAsync(userId, page, pageSize);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
    {
        var userId = GetUserId();
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }
        
        // ✅ SECURITY: Authorization check - Kullanıcı sadece kendi siparişlerine erişebilmeli
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        return Ok(order);
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var order = await _orderService.CreateOrderFromCartAsync(userId, dto.ShippingAddressId, dto.CouponCode);
        if (order == null)
        {
            return BadRequest("Sipariş oluşturulamadı.");
        }
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var statusEnum = Enum.Parse<OrderStatus>(dto.Status);
        var order = await _orderService.UpdateOrderStatusAsync(id, statusEnum);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var userId = GetUserId();
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }
        
        // ✅ SECURITY: Authorization check - Kullanıcı sadece kendi siparişlerini iptal edebilmeli
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        var result = await _orderService.CancelOrderAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/reorder")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderDto>> Reorder(Guid id)
    {
        var userId = GetUserId();
        
        // ✅ SECURITY: Authorization check - Kullanıcı sadece kendi siparişlerini yeniden sipariş edebilmeli
        var originalOrder = await _orderService.GetByIdAsync(id);
        if (originalOrder == null)
        {
            return NotFound();
        }
        
        if (originalOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        var order = await _orderService.ReorderAsync(id, userId);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    [HttpPost("filter")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> FilterOrders([FromBody] OrderFilterDto filter)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        filter.UserId = userId;
        var orders = await _orderFilterService.GetFilteredOrdersAsync(filter);
        return Ok(orders);
    }

    [HttpGet("statistics")]
    [ProducesResponseType(typeof(OrderStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OrderStatisticsDto>> GetStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var userId = GetUserId();
        var stats = await _orderFilterService.GetOrderStatisticsAsync(userId, startDate, endDate);
        return Ok(stats);
    }

    [HttpPost("export/csv")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportToCsv([FromBody] OrderExportDto exportDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var csvData = await _orderService.ExportOrdersToCsvAsync(exportDto);
        return File(csvData, "text/csv", $"orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    [HttpPost("export/json")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportToJson([FromBody] OrderExportDto exportDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var jsonData = await _orderService.ExportOrdersToJsonAsync(exportDto);
        return File(jsonData, "application/json", $"orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
    }

    [HttpPost("export/excel")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportToExcel([FromBody] OrderExportDto exportDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var excelData = await _orderService.ExportOrdersToExcelAsync(exportDto);
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                   $"orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }

}

