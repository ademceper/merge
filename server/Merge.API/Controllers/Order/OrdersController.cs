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
using FilterOrdersQuery = Merge.Application.Order.Queries.FilterOrders.FilterOrdersQuery;
using GetOrderStatisticsQuery = Merge.Application.Order.Queries.GetOrderStatistics.GetOrderStatisticsQuery;
using Merge.Domain.Enums;
using Merge.API.Middleware;
using Merge.API.Extensions;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Order;

/// <summary>
/// Order API endpoints.
/// Tüm sipariş operasyonlarını yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/orders")]
[Authorize]
[Tags("Orders")]
public class OrdersController(
    IMediator mediator,
    IOptions<OrderSettings> orderSettings) : BaseController
{
    private readonly OrderSettings _orderSettings = orderSettings.Value;
    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetMyOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _orderSettings.MaxPageSize) pageSize = _orderSettings.MaxPageSize;
        if (page < 1) page = 1;
        var userId = GetUserId();
        var query = new GetOrdersByUserIdQuery(userId, page, pageSize);
        var orders = await mediator.Send(query, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetOrderByIdQuery(id);
        var order = await mediator.Send(query, cancellationToken);
        if (order is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var orderJson = System.Text.Json.JsonSerializer.Serialize(order);
        Response.SetETag(orderJson);
        Response.SetCacheControl(maxAgeSeconds: 60, isPublic: false); // Cache for 1 minute (private)

        // Check if client has cached version (304 Not Modified)
        var etag = Response.Headers["ETag"].FirstOrDefault();
        if (!string.IsNullOrEmpty(etag) && Request.IsNotModified(etag))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        return Ok(order);
    }

    [HttpPost]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderDto>> CreateOrder(
        [FromBody] CreateOrderDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        var userId = GetUserId();
        var command = new CreateOrderFromCartCommand(userId, dto.ShippingAddressId, dto.CouponCode);
        var order = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
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
        if (validationResult is not null) return validationResult;
        var statusEnum = Enum.Parse<OrderStatus>(dto.Status);
        var command = new UpdateOrderStatusCommand(id, statusEnum);
        var order = await mediator.Send(command, cancellationToken);
        return Ok(order);
    }

    /// <summary>
    /// Sipariş durumunu kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderDto>> PatchStatus(
        Guid id,
        [FromBody] PatchOrderStatusDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        if (!patchDto.Status.HasValue)
        {
            return BadRequest("Status güncellenmelidir.");
        }
        var command = new UpdateOrderStatusCommand(id, patchDto.Status.Value);
        var order = await mediator.Send(command, cancellationToken);
        return Ok(order);
    }

    [HttpPost("{id}/cancel")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var getOrderQuery = new GetOrderByIdQuery(id);
        var order = await mediator.Send(getOrderQuery, cancellationToken);
        if (order is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var command = new CancelOrderCommand(id);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpPost("{id}/reorder")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderDto>> Reorder(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var getOrderQuery = new GetOrderByIdQuery(id);
        var originalOrder = await mediator.Send(getOrderQuery, cancellationToken);
        if (originalOrder is null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        if (originalOrder.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        var command = new ReorderCommand(id, userId);
        var order = await mediator.Send(command, cancellationToken);
        return Ok(order);
    }

    [HttpPost("filter")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<OrderDto>>> FilterOrders(
        [FromBody] OrderFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult is not null) return validationResult;
        if (filter.PageSize > _orderSettings.MaxPageSize) filter.PageSize = _orderSettings.MaxPageSize;
        if (filter.Page < 1) filter.Page = 1;
        var userId = GetUserId();
        var query = new FilterOrdersQuery(
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
        var orders = await mediator.Send(query, cancellationToken);
        return Ok(orders);
    }

    [HttpGet("statistics")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(OrderStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<OrderStatisticsDto>> GetStatistics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetOrderStatisticsQuery(
            userId, startDate, endDate);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    [HttpPost("export/csv")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)]
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
        if (validationResult is not null) return validationResult;
        var command = new ExportOrdersCommand(
            exportDto.StartDate,
            exportDto.EndDate,
            exportDto.Status,
            exportDto.PaymentStatus,
            exportDto.UserId,
            exportDto.IncludeOrderItems,
            exportDto.IncludeAddress,
            ExportFormat.Csv);
        var csvData = await mediator.Send(command, cancellationToken);
        return File(csvData, "text/csv", $"orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    [HttpPost("export/json")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)]
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
        if (validationResult is not null) return validationResult;
        var command = new ExportOrdersCommand(
            exportDto.StartDate,
            exportDto.EndDate,
            exportDto.Status,
            exportDto.PaymentStatus,
            exportDto.UserId,
            exportDto.IncludeOrderItems,
            exportDto.IncludeAddress,
            ExportFormat.Json);
        var jsonData = await mediator.Send(command, cancellationToken);
        return File(jsonData, "application/json", $"orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
    }

    [HttpPost("export/excel")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(5, 60)]
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
        if (validationResult is not null) return validationResult;
        var command = new ExportOrdersCommand(
            exportDto.StartDate,
            exportDto.EndDate,
            exportDto.Status,
            exportDto.PaymentStatus,
            exportDto.UserId,
            exportDto.IncludeOrderItems,
            exportDto.IncludeAddress,
            ExportFormat.Excel);
        var excelData = await mediator.Send(command, cancellationToken);
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                   $"orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }

}
