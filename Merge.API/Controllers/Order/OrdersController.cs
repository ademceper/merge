using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;
using Merge.Domain.Enums;
using Merge.API.Middleware;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
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

    /// <summary>
    /// Kullanıcının siparişlerini getirir (pagination ile)
    /// </summary>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetMyOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var orders = await _orderService.GetOrdersByUserIdAsync(userId, page, pageSize, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Sipariş detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
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

    /// <summary>
    /// Sepetten sipariş oluşturur
    /// </summary>
    [HttpPost]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika (Fraud koruması)
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var order = await _orderService.CreateOrderFromCartAsync(userId, dto.ShippingAddressId, dto.CouponCode, cancellationToken);
        if (order == null)
        {
            return BadRequest("Sipariş oluşturulamadı.");
        }
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    /// <summary>
    /// Sipariş durumunu günceller (Admin)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateOrderStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var statusEnum = Enum.Parse<OrderStatus>(dto.Status);
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var order = await _orderService.UpdateOrderStatusAsync(id, statusEnum, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    /// <summary>
    /// Siparişi iptal eder
    /// </summary>
    [HttpPost("{id}/cancel")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var order = await _orderService.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }
        
        // ✅ SECURITY: Authorization check - Kullanıcı sadece kendi siparişlerini iptal edebilmeli
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var result = await _orderService.CancelOrderAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Siparişi yeniden sipariş eder
    /// </summary>
    [HttpPost("{id}/reorder")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderDto>> Reorder(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        // ✅ SECURITY: Authorization check - Kullanıcı sadece kendi siparişlerini yeniden sipariş edebilmeli
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var originalOrder = await _orderService.GetByIdAsync(id, cancellationToken);
        if (originalOrder == null)
        {
            return NotFound();
        }
        
        if (originalOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var order = await _orderService.ReorderAsync(id, userId, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    /// <summary>
    /// Siparişleri filtreler (pagination ile)
    /// </summary>
    [HttpPost("filter")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<OrderDto>>> FilterOrders(
        [FromBody] OrderFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (filter.PageSize > 100) filter.PageSize = 100;
        if (filter.Page < 1) filter.Page = 1;

        var userId = GetUserId();
        filter.UserId = userId;
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var orders = await _orderFilterService.GetFilteredOrdersAsync(filter, cancellationToken);
        
        // ✅ BOLUM 3.4: Pagination - IEnumerable yerine PagedResult döndür
        var ordersList = orders.ToList();
        var totalCount = ordersList.Count; // Not: Filter service'de totalCount hesaplanmalı, şimdilik bu şekilde
        
        return Ok(new PagedResult<OrderDto>
        {
            Items = ordersList,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
    }

    /// <summary>
    /// Kullanıcının sipariş istatistiklerini getirir
    /// </summary>
    [HttpGet("statistics")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(OrderStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderStatisticsDto>> GetStatistics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var stats = await _orderFilterService.GetOrderStatisticsAsync(userId, startDate, endDate, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Siparişleri CSV formatında export eder (Admin, Manager)
    /// </summary>
    [HttpPost("export/csv")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5/dakika (Heavy operation)
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ExportToCsv(
        [FromBody] OrderExportDto exportDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var csvData = await _orderService.ExportOrdersToCsvAsync(exportDto, cancellationToken);
        return File(csvData, "text/csv", $"orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Siparişleri JSON formatında export eder (Admin, Manager)
    /// </summary>
    [HttpPost("export/json")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5/dakika (Heavy operation)
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ExportToJson(
        [FromBody] OrderExportDto exportDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var jsonData = await _orderService.ExportOrdersToJsonAsync(exportDto, cancellationToken);
        return File(jsonData, "application/json", $"orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
    }

    /// <summary>
    /// Siparişleri Excel formatında export eder (Admin, Manager)
    /// </summary>
    [HttpPost("export/excel")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5/dakika (Heavy operation)
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ExportToExcel(
        [FromBody] OrderExportDto exportDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var excelData = await _orderService.ExportOrdersToExcelAsync(exportDto, cancellationToken);
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                   $"orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }

}

