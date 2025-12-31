using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.DTOs.Logistics;


namespace Merge.API.Controllers.Logistics;

[ApiController]
[Route("api/logistics/pick-packs")]
[Authorize(Roles = "Admin,Manager,Warehouse")]
public class PickPacksController : BaseController
{
    private readonly IPickPackService _pickPackService;

    public PickPacksController(IPickPackService pickPackService)
    {
        _pickPackService = pickPackService;
    }

    [HttpPost]
    public async Task<ActionResult<PickPackDto>> CreatePickPack([FromBody] CreatePickPackDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var pickPack = await _pickPackService.CreatePickPackAsync(dto);
        return CreatedAtAction(nameof(GetPickPack), new { id = pickPack.Id }, pickPack);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PickPackDto>> GetPickPack(Guid id)
    {
        var pickPack = await _pickPackService.GetPickPackByIdAsync(id);
        if (pickPack == null)
        {
            return NotFound();
        }
        return Ok(pickPack);
    }

    [HttpGet("pack-number/{packNumber}")]
    public async Task<ActionResult<PickPackDto>> GetPickPackByPackNumber(string packNumber)
    {
        var pickPack = await _pickPackService.GetPickPackByPackNumberAsync(packNumber);
        if (pickPack == null)
        {
            return NotFound();
        }
        return Ok(pickPack);
    }

    [HttpGet("order/{orderId}")]
    public async Task<ActionResult<IEnumerable<PickPackDto>>> GetPickPacksByOrder(Guid orderId)
    {
        var pickPacks = await _pickPackService.GetPickPacksByOrderIdAsync(orderId);
        return Ok(pickPacks);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PickPackDto>>> GetAllPickPacks(
        [FromQuery] string? status = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var pickPacks = await _pickPackService.GetAllPickPacksAsync(status, warehouseId, page, pageSize);
        return Ok(pickPacks);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdatePickPackStatusDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserIdOrNull();
        var success = await _pickPackService.UpdatePickPackStatusAsync(id, dto, userId);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/start-picking")]
    public async Task<IActionResult> StartPicking(Guid id)
    {
        var userId = GetUserId();
        var success = await _pickPackService.StartPickingAsync(id, userId);
        if (!success)
        {
            return BadRequest();
        }
        return NoContent();
    }

    [HttpPost("{id}/complete-picking")]
    public async Task<IActionResult> CompletePicking(Guid id)
    {
        var userId = GetUserId();
        var success = await _pickPackService.CompletePickingAsync(id, userId);
        if (!success)
        {
            return BadRequest();
        }
        return NoContent();
    }

    [HttpPost("{id}/start-packing")]
    public async Task<IActionResult> StartPacking(Guid id)
    {
        var userId = GetUserId();
        var success = await _pickPackService.StartPackingAsync(id, userId);
        if (!success)
        {
            return BadRequest();
        }
        return NoContent();
    }

    [HttpPost("{id}/complete-packing")]
    public async Task<IActionResult> CompletePacking(Guid id)
    {
        var userId = GetUserId();
        var success = await _pickPackService.CompletePackingAsync(id, userId);
        if (!success)
        {
            return BadRequest();
        }
        return NoContent();
    }

    [HttpPost("{id}/mark-shipped")]
    public async Task<IActionResult> MarkAsShipped(Guid id)
    {
        var success = await _pickPackService.MarkAsShippedAsync(id);
        if (!success)
        {
            return BadRequest();
        }
        return NoContent();
    }

    [HttpPut("items/{itemId}/status")]
    public async Task<IActionResult> UpdateItemStatus(Guid itemId, [FromBody] PickPackItemStatusDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _pickPackService.UpdatePickPackItemStatusAsync(itemId, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("stats")]
    public async Task<ActionResult<Dictionary<string, int>>> GetStats(
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var stats = await _pickPackService.GetPickPackStatsAsync(warehouseId, startDate, endDate);
        return Ok(stats);
    }
}

