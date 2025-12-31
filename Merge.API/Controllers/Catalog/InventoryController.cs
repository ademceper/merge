using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Catalog;
using Merge.Application.DTOs.Logistics;


namespace Merge.API.Controllers.Catalog;

[ApiController]
[Route("api/catalog/inventory")]
[Authorize(Roles = "Admin,Seller")]
public class InventoryController : BaseController
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryDto>> GetById(Guid id)
    {
        var inventory = await _inventoryService.GetByIdAsync(id);
        if (inventory == null)
        {
            return NotFound();
        }
        return Ok(inventory);
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetByProduct(Guid productId)
    {
        var inventories = await _inventoryService.GetByProductIdAsync(productId);
        return Ok(inventories);
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetByWarehouse(Guid warehouseId)
    {
        var inventories = await _inventoryService.GetByWarehouseIdAsync(warehouseId);
        return Ok(inventories);
    }

    [HttpGet("product/{productId}/warehouse/{warehouseId}")]
    public async Task<ActionResult<InventoryDto>> GetByProductAndWarehouse(Guid productId, Guid warehouseId)
    {
        var inventory = await _inventoryService.GetByProductAndWarehouseAsync(productId, warehouseId);
        if (inventory == null)
        {
            return NotFound();
        }
        return Ok(inventory);
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<LowStockAlertDto>>> GetLowStockAlerts([FromQuery] Guid? warehouseId = null)
    {
        var alerts = await _inventoryService.GetLowStockAlertsAsync(warehouseId);
        return Ok(alerts);
    }

    [HttpGet("report/product/{productId}")]
    public async Task<ActionResult<StockReportDto>> GetStockReport(Guid productId)
    {
        var report = await _inventoryService.GetStockReportByProductAsync(productId);
        if (report == null)
        {
            return NotFound();
        }
        return Ok(report);
    }

    [HttpGet("available/{productId}")]
    public async Task<ActionResult<int>> GetAvailableStock(Guid productId, [FromQuery] Guid? warehouseId = null)
    {
        var availableStock = await _inventoryService.GetAvailableStockAsync(productId, warehouseId);
        return Ok(new { productId, warehouseId, availableStock });
    }

    [HttpPost]
    public async Task<ActionResult<InventoryDto>> Create([FromBody] CreateInventoryDto createDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var inventory = await _inventoryService.CreateAsync(createDto);
        return CreatedAtAction(nameof(GetById), new { id = inventory.Id }, inventory);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<InventoryDto>> Update(Guid id, [FromBody] UpdateInventoryDto updateDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var inventory = await _inventoryService.UpdateAsync(id, updateDto);
        if (inventory == null)
        {
            return NotFound();
        }
        return Ok(inventory);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _inventoryService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("adjust")]
    public async Task<ActionResult<InventoryDto>> AdjustStock([FromBody] AdjustInventoryDto adjustDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var inventory = await _inventoryService.AdjustStockAsync(adjustDto, userId);
        return Ok(inventory);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> TransferStock([FromBody] TransferInventoryDto transferDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var result = await _inventoryService.TransferStockAsync(transferDto, userId);
        return NoContent();
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveStock([FromBody] ReserveStockDto reserveDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _inventoryService.ReserveStockAsync(
            reserveDto.ProductId,
            reserveDto.WarehouseId,
            reserveDto.Quantity,
            reserveDto.OrderId
        );
        return NoContent();
    }

    [HttpPost("release")]
    public async Task<IActionResult> ReleaseStock([FromBody] ReleaseStockDto releaseDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _inventoryService.ReleaseStockAsync(
            releaseDto.ProductId,
            releaseDto.WarehouseId,
            releaseDto.Quantity,
            releaseDto.OrderId
        );
        return NoContent();
    }

    [HttpPost("{id}/update-count-date")]
    public async Task<IActionResult> UpdateLastCountDate(Guid id)
    {
        var result = await _inventoryService.UpdateLastCountDateAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
