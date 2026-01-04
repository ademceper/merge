using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Interfaces.Order;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Logistics;

[ApiController]
[Route("api/logistics/pick-packs")]
[Authorize(Roles = "Admin,Manager,Warehouse")]
public class PickPacksController : BaseController
{
    private readonly IPickPackService _pickPackService;
    private readonly IOrderService _orderService;

    public PickPacksController(IPickPackService pickPackService, IOrderService orderService)
    {
        _pickPackService = pickPackService;
        _orderService = orderService;
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

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var pickPack = await _pickPackService.CreatePickPackAsync(dto, cancellationToken);
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

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var pickPack = await _pickPackService.GetPickPackByIdAsync(id, cancellationToken);
        if (pickPack == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi siparişlerinin pick-pack'lerine erişebilmeli
        var order = await _orderService.GetByIdAsync(pickPack.OrderId, cancellationToken);
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

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var pickPack = await _pickPackService.GetPickPackByPackNumberAsync(packNumber, cancellationToken);
        if (pickPack == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi siparişlerinin pick-pack'lerine erişebilmeli
        var order = await _orderService.GetByIdAsync(pickPack.OrderId, cancellationToken);
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
        var order = await _orderService.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Warehouse"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var pickPacks = await _pickPackService.GetPickPacksByOrderIdAsync(orderId, cancellationToken);
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

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var pickPacks = await _pickPackService.GetAllPickPacksAsync(status, warehouseId, page, pageSize, cancellationToken);
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

        var userId = GetUserIdOrNull();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _pickPackService.UpdatePickPackStatusAsync(id, dto, userId, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _pickPackService.StartPickingAsync(id, userId, cancellationToken);
        if (!success)
        {
            return BadRequest();
        }
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
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _pickPackService.CompletePickingAsync(id, userId, cancellationToken);
        if (!success)
        {
            return BadRequest();
        }
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _pickPackService.StartPackingAsync(id, userId, cancellationToken);
        if (!success)
        {
            return BadRequest();
        }
        return NoContent();
    }

    /// <summary>
    /// Pack işlemini tamamlar
    /// </summary>
    [HttpPost("{id}/complete-packing")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CompletePacking(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _pickPackService.CompletePackingAsync(id, userId, cancellationToken);
        if (!success)
        {
            return BadRequest();
        }
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _pickPackService.MarkAsShippedAsync(id, cancellationToken);
        if (!success)
        {
            return BadRequest();
        }
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

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _pickPackService.UpdatePickPackItemStatusAsync(itemId, dto, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var stats = await _pickPackService.GetPickPackStatsAsync(warehouseId, startDate, endDate, cancellationToken);
        return Ok(stats);
    }
}

