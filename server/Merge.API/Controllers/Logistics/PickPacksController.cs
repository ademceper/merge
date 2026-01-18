using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.API.Middleware;
using Merge.Application.Logistics.Commands.CreatePickPack;
using Merge.Application.Logistics.Queries.GetPickPackById;
using Merge.Application.Logistics.Queries.GetPickPackByPackNumber;
using Merge.Application.Logistics.Queries.GetPickPacksByOrderId;
using Merge.Application.Logistics.Queries.GetAllPickPacks;
using Merge.Application.Logistics.Queries.GetPickPackStats;
using Merge.Application.Logistics.Commands.UpdatePickPackDetails;
using Merge.Application.Logistics.Commands.StartPicking;
using Merge.Application.Logistics.Commands.CompletePicking;
using Merge.Application.Logistics.Commands.StartPacking;
using Merge.Application.Logistics.Commands.CompletePacking;
using Merge.Application.Logistics.Commands.MarkPickPackAsShipped;
using Merge.Application.Logistics.Commands.UpdatePickPackItemStatus;
using Merge.Application.Order.Queries.GetOrderById;
using Merge.Domain.Enums;

namespace Merge.API.Controllers.Logistics;

/// <summary>
/// Pick & Pack API endpoints.
/// Toplama ve paketleme işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/logistics/pick-packs")]
[Authorize(Roles = "Admin,Manager,Warehouse")]
[Tags("PickPacks")]
public class PickPacksController(
    IMediator mediator,
    IOptions<ShippingSettings> shippingSettings) : BaseController
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    [HttpPost]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(PickPackDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PickPackDto>> CreatePickPack(
        [FromBody] CreatePickPackDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new CreatePickPackCommand(dto.OrderId, dto.WarehouseId, dto.Notes);
        var pickPack = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPickPack), new { id = pickPack.Id }, pickPack);
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PickPackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PickPackDto>> GetPickPack(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var query = new GetPickPackByIdQuery(id);
        var pickPack = await mediator.Send(query, cancellationToken);
        if (pickPack == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        var orderQuery = new GetOrderByIdQuery(pickPack.OrderId);
        var order = await mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Warehouse"))
        {
            return Forbid();
        }

        return Ok(pickPack);
    }

    [HttpGet("pack-number/{packNumber}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PickPackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PickPackDto>> GetPickPackByPackNumber(
        string packNumber,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var query = new GetPickPackByPackNumberQuery(packNumber);
        var pickPack = await mediator.Send(query, cancellationToken);
        if (pickPack == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        var orderQuery = new GetOrderByIdQuery(pickPack.OrderId);
        var order = await mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Warehouse"))
        {
            return Forbid();
        }

        return Ok(pickPack);
    }

    [HttpGet("order/{orderId}")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<PickPackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<PickPackDto>>> GetPickPacksByOrder(
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
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Warehouse"))
        {
            return Forbid();
        }

        var query = new GetPickPacksByOrderIdQuery(orderId);
        var pickPacks = await mediator.Send(query, cancellationToken);
        return Ok(pickPacks);
    }

    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(PagedResult<PickPackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PickPackDto>>> GetAllPickPacks(
        [FromQuery] string? status = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (pageSize > _shippingSettings.QueryLimits.MaxPageSize) 
            pageSize = _shippingSettings.QueryLimits.MaxPageSize;

        PickPackStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PickPackStatus>(status, out var parsedStatus))
        {
            statusEnum = parsedStatus;
        }

        var query = new GetAllPickPacksQuery(statusEnum, warehouseId, page, pageSize);
        var pickPacks = await mediator.Send(query, cancellationToken);
        return Ok(pickPacks);
    }

    [HttpPut("{id}/details")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateDetails(
        Guid id,
        [FromBody] UpdatePickPackStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // Bu endpoint sadece details (notes, weight, dimensions, packageCount) update için kullanılır
        var command = new UpdatePickPackDetailsCommand(id, dto.Notes, dto.Weight, dto.Dimensions, dto.PackageCount);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pick pack detaylarını kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}/details")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PatchDetails(
        Guid id,
        [FromBody] PatchPickPackDetailsDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new UpdatePickPackDetailsCommand(id, patchDto.Notes, patchDto.Weight, patchDto.Dimensions, patchDto.PackageCount);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/start-picking")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> StartPicking(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new StartPickingCommand(id, userId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/complete-picking")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CompletePicking(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new CompletePickingCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/start-packing")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> StartPacking(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new StartPackingCommand(id, userId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/complete-packing")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CompletePacking(
        Guid id,
        [FromBody] CompletePackingDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new CompletePackingCommand(id, dto.Weight, dto.Dimensions, dto.PackageCount);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/mark-shipped")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkAsShipped(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new MarkPickPackAsShippedCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPut("items/{itemId}/status")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateItemStatus(
        Guid itemId,
        [FromBody] PickPackItemStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new UpdatePickPackItemStatusCommand(itemId, dto.IsPicked, dto.IsPacked, dto.Location);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pick pack öğe durumunu kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("items/{itemId}/status")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PatchItemStatus(
        Guid itemId,
        [FromBody] PatchPickPackItemStatusDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new UpdatePickPackItemStatusCommand(itemId, patchDto.IsPicked, patchDto.IsPacked, patchDto.Location);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("stats")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<Dictionary<string, int>>> GetStats(
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPickPackStatsQuery(warehouseId, startDate, endDate);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
