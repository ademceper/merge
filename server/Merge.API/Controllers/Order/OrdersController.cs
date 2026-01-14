using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;
using Merge.Application.Order.Commands.CreateOrderFromCart;
using Merge.Application.Order.Commands.CancelOrder;
using Merge.Application.Order.Commands.UpdateOrderStatus;
using Merge.Application.Order.Commands.Reorder;
using Merge.Application.Order.Commands.ExportOrders;
using Merge.Application.Order.Queries.GetOrderById;
using Merge.Application.Order.Queries.GetOrdersByUserId;
using Merge.Application.Order.Queries.FilterOrders;
using Merge.Application.Order.Queries.GetOrderStatistics;
using Merge.Domain.Enums;
using Merge.API.Middleware;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
namespace Merge.API.Controllers.Order;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : BaseController
{
    private readonly IMediator _mediator;
    private readonly OrderSettings _orderSettings;

    public OrdersController(
        IMediator mediator,
        IOptions<OrderSettings> orderSettings)
    {
        _mediator = mediator;
        _orderSettings = orderSettings.Value;
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
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Configuration'dan al
        if (pageSize > _orderSettings.MaxPageSize) pageSize = _orderSettings.MaxPageSize;
        if (page < 1) page = 1;

        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetOrdersByUserIdQuery(userId, page, pageSize);
        var orders = await _mediator.Send(query, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new GetOrderByIdQuery(id);
        var order = await _mediator.Send(query, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new CreateOrderFromCartCommand(userId, dto.ShippingAddressId, dto.CouponCode);
        var order = await _mediator.Send(command, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new UpdateOrderStatusCommand(id, statusEnum);
        var order = await _mediator.Send(command, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        // Authorization check için önce order'ı getir
        var getOrderQuery = new GetOrderByIdQuery(id);
        var order = await _mediator.Send(getOrderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }
        
        // ✅ SECURITY: Authorization check - Kullanıcı sadece kendi siparişlerini iptal edebilmeli
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new CancelOrderCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var getOrderQuery = new GetOrderByIdQuery(id);
        var originalOrder = await _mediator.Send(getOrderQuery, cancellationToken);
        if (originalOrder == null)
        {
            return NotFound();
        }
        
        if (originalOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new ReorderCommand(id, userId);
        var order = await _mediator.Send(command, cancellationToken);
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

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Configuration'dan al
        if (filter.PageSize > _orderSettings.MaxPageSize) filter.PageSize = _orderSettings.MaxPageSize;
        if (filter.Page < 1) filter.Page = 1;

        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new Merge.Application.Order.Queries.FilterOrders.FilterOrdersQuery(
            UserId: userId,
            Status: filter.Status,
            PaymentStatus: filter.PaymentStatus,
            StartDate: filter.StartDate,
            EndDate: filter.EndDate,
            MinAmount: filter.MinAmount,
            MaxAmount: filter.MaxAmount,
            OrderNumber: filter.OrderNumber,
            Page: filter.Page,
            PageSize: filter.PageSize,
            SortBy: filter.SortBy,
            SortDescending: filter.SortDescending);
        var orders = await _mediator.Send(query, cancellationToken);
        return Ok(orders);
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var query = new Merge.Application.Order.Queries.GetOrderStatistics.GetOrderStatisticsQuery(
            userId, startDate, endDate);
        var stats = await _mediator.Send(query, cancellationToken);
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new ExportOrdersCommand(
            exportDto.StartDate,
            exportDto.EndDate,
            exportDto.Status,
            exportDto.PaymentStatus,
            exportDto.UserId,
            exportDto.IncludeOrderItems,
            exportDto.IncludeAddress,
            ExportFormat.Csv);
        var csvData = await _mediator.Send(command, cancellationToken);
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new ExportOrdersCommand(
            exportDto.StartDate,
            exportDto.EndDate,
            exportDto.Status,
            exportDto.PaymentStatus,
            exportDto.UserId,
            exportDto.IncludeOrderItems,
            exportDto.IncludeAddress,
            ExportFormat.Json);
        var jsonData = await _mediator.Send(command, cancellationToken);
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU) - Service layer bypass
        var command = new ExportOrdersCommand(
            exportDto.StartDate,
            exportDto.EndDate,
            exportDto.Status,
            exportDto.PaymentStatus,
            exportDto.UserId,
            exportDto.IncludeOrderItems,
            exportDto.IncludeAddress,
            ExportFormat.Excel);
        var excelData = await _mediator.Send(command, cancellationToken);
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                   $"orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }

}

