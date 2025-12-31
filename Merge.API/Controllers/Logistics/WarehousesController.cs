using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.DTOs.Logistics;


namespace Merge.API.Controllers.Logistics;

[ApiController]
[Route("api/logistics/warehouses")]
[Authorize(Roles = "Admin")]
public class WarehousesController : BaseController
{
    private readonly IWarehouseService _warehouseService;

    public WarehousesController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetAll([FromQuery] bool includeInactive = false)
    {
        var warehouses = await _warehouseService.GetAllAsync(includeInactive);
        return Ok(warehouses);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetActive()
    {
        var warehouses = await _warehouseService.GetActiveWarehousesAsync();
        return Ok(warehouses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WarehouseDto>> GetById(Guid id)
    {
        var warehouse = await _warehouseService.GetByIdAsync(id);
        if (warehouse == null)
        {
            return NotFound();
        }
        return Ok(warehouse);
    }

    [HttpGet("code/{code}")]
    public async Task<ActionResult<WarehouseDto>> GetByCode(string code)
    {
        var warehouse = await _warehouseService.GetByCodeAsync(code);
        if (warehouse == null)
        {
            return NotFound();
        }
        return Ok(warehouse);
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseDto>> Create([FromBody] CreateWarehouseDto createDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var warehouse = await _warehouseService.CreateAsync(createDto);
        return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WarehouseDto>> Update(Guid id, [FromBody] UpdateWarehouseDto updateDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var warehouse = await _warehouseService.UpdateAsync(id, updateDto);
        if (warehouse == null)
        {
            return NotFound();
        }
        return Ok(warehouse);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _warehouseService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await _warehouseService.ActivateAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var result = await _warehouseService.DeactivateAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
