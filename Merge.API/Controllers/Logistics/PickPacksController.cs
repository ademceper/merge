using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;
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

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/logistics/pick-packs")]
[Authorize(Roles = "Admin,Manager,Warehouse")]
public class PickPacksController : BaseController
{
    private readonly IMediator _mediator;

    public PickPacksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Yeni pick-pack kaydı oluşturur
    /// </summary>
    [HttpPost]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(PickPackDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PickPackDto>> CreatePickPack(
        [FromBody] CreatePickPackDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new CreatePickPackCommand(dto.OrderId, dto.WarehouseId, dto.Notes);
        var pickPack = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPickPack), new { id = pickPack.Id }, pickPack);
    }

    /// <summary>
    /// Pick-pack detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
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
        var pickPack = await _mediator.Send(query, cancellationToken);
        if (pickPack == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi siparişlerinin pick-pack'lerine erişebilmeli
        var orderQuery = new GetOrderByIdQuery(pickPack.OrderId);
        var order = await _mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Warehouse"))
        {
            return Forbid();
        }

        return Ok(pickPack);
    }

    /// <summary>
    /// Paket numarasına göre pick-pack getirir
    /// </summary>
    [HttpGet("pack-number/{packNumber}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
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
        var pickPack = await _mediator.Send(query, cancellationToken);
        if (pickPack == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi siparişlerinin pick-pack'lerine erişebilmeli
        var orderQuery = new GetOrderByIdQuery(pickPack.OrderId);
        var order = await _mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Warehouse"))
        {
            return Forbid();
        }

        return Ok(pickPack);
    }

    /// <summary>
    /// Siparişe ait pick-pack'leri getirir
    /// </summary>
    [HttpGet("order/{orderId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
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

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi siparişlerinin pick-pack'lerine erişebilmeli
        var orderQuery = new GetOrderByIdQuery(orderId);
        var order = await _mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Warehouse"))
        {
            return Forbid();
        }

        var query = new GetPickPacksByOrderIdQuery(orderId);
        var pickPacks = await _mediator.Send(query, cancellationToken);
        return Ok(pickPacks);
    }

    /// <summary>
    /// Tüm pick-pack'leri getirir (pagination ile)
    /// </summary>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
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
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit

        PickPackStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PickPackStatus>(status, out var parsedStatus))
        {
            statusEnum = parsedStatus;
        }

        var query = new GetAllPickPacksQuery(statusEnum, warehouseId, page, pageSize);
        var pickPacks = await _mediator.Send(query, cancellationToken);
        return Ok(pickPacks);
    }

    /// <summary>
    /// Pick-pack durumunu günceller
    /// </summary>
    [HttpPut("{id}/status")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdatePickPackStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        PickPackStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse<PickPackStatus>(dto.Status, out var parsedStatus))
        {
            statusEnum = parsedStatus;
        }

        var command = new UpdatePickPackDetailsCommand(id, statusEnum, dto.Notes, dto.Weight, dto.Dimensions, dto.PackageCount);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pick işlemini başlatır
    /// </summary>
    [HttpPost("{id}/start-picking")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> StartPicking(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new StartPickingCommand(id, userId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pick işlemini tamamlar
    /// </summary>
    [HttpPost("{id}/complete-picking")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CompletePicking(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new CompletePickingCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pack işlemini başlatır
    /// </summary>
    [HttpPost("{id}/start-packing")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> StartPacking(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new StartPackingCommand(id, userId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pack işlemini tamamlar
    /// </summary>
    [HttpPost("{id}/complete-packing")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CompletePacking(
        Guid id,
        [FromBody] CompletePackingDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new CompletePackingCommand(id, dto.Weight, dto.Dimensions, dto.PackageCount);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pick-pack'i kargoya verildi olarak işaretler
    /// </summary>
    [HttpPost("{id}/mark-shipped")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkAsShipped(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new MarkPickPackAsShippedCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pick-pack item durumunu günceller
    /// </summary>
    [HttpPut("items/{itemId}/status")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
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
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pick-pack istatistiklerini getirir
    /// </summary>
    [HttpGet("stats")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    // ⚠️ NOTE: Dictionary<string, int> burada kabul edilebilir çünkü stats için key-value çiftleri dinamik
    public async Task<ActionResult<Dictionary<string, int>>> GetStats(
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPickPackStatsQuery(warehouseId, startDate, endDate);
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}

