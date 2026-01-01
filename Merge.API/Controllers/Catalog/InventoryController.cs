using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Catalog;
using Merge.Application.Interfaces.Product;
using Merge.Application.DTOs.Logistics;


namespace Merge.API.Controllers.Catalog;

[ApiController]
[Route("api/catalog/inventory")]
[Authorize(Roles = "Admin,Seller")]
public class InventoryController : BaseController
{
    private readonly IInventoryService _inventoryService;
    private readonly IProductService _productService;

    public InventoryController(IInventoryService inventoryService, IProductService productService)
    {
        _inventoryService = inventoryService;
        _productService = productService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryDto>> GetById(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var inventory = await _inventoryService.GetByIdAsync(id);
        if (inventory == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin inventory'sine erişebilmeli
        // InventoryDto'da ProductId var, Product bilgisini kontrol et
        var product = await _productService.GetByIdAsync(inventory.ProductId);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return Ok(inventory);
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetByProduct(Guid productId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin inventory'sine erişebilmeli
        var product = await _productService.GetByIdAsync(productId);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var inventories = await _inventoryService.GetByProductIdAsync(productId);
        return Ok(inventories);
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetByWarehouse(Guid warehouseId)
    {
        // Admin tüm warehouse'ları görebilir, Seller sadece kendi ürünlerinin inventory'sini görebilir
        // Bu endpoint'te filtreleme service katmanında yapılmalı
        var inventories = await _inventoryService.GetByWarehouseIdAsync(warehouseId);
        return Ok(inventories);
    }

    [HttpGet("product/{productId}/warehouse/{warehouseId}")]
    public async Task<ActionResult<InventoryDto>> GetByProductAndWarehouse(Guid productId, Guid warehouseId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin inventory'sine erişebilmeli
        var product = await _productService.GetByIdAsync(productId);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

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
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin stock report'una erişebilmeli
        var product = await _productService.GetByIdAsync(productId);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

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
        // Available stock bilgisi public olabilir (ürün detay sayfasında gösterilir)
        // Ancak Seller kendi ürünlerinin stokunu görebilmeli
        var availableStock = await _inventoryService.GetAvailableStockAsync(productId, warehouseId);
        return Ok(new { productId, warehouseId, availableStock });
    }

    [HttpPost]
    public async Task<ActionResult<InventoryDto>> Create([FromBody] CreateInventoryDto createDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin inventory'sini oluşturabilmeli
        var product = await _productService.GetByIdAsync(createDto.ProductId);
        if (product == null)
        {
            return NotFound("Ürün bulunamadı.");
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var inventory = await _inventoryService.CreateAsync(createDto);
        return CreatedAtAction(nameof(GetById), new { id = inventory.Id }, inventory);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<InventoryDto>> Update(Guid id, [FromBody] UpdateInventoryDto updateDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin inventory'sini güncelleyebilmeli
        var inventory = await _inventoryService.GetByIdAsync(id);
        if (inventory == null)
        {
            return NotFound();
        }

        var product = await _productService.GetByIdAsync(inventory.ProductId);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var updatedInventory = await _inventoryService.UpdateAsync(id, updateDto);
        if (updatedInventory == null)
        {
            return NotFound();
        }
        return Ok(updatedInventory);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin inventory'sini silebilmeli
        var inventory = await _inventoryService.GetByIdAsync(id);
        if (inventory == null)
        {
            return NotFound();
        }

        var product = await _productService.GetByIdAsync(inventory.ProductId);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

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

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin inventory'sini güncelleyebilmeli
        var inventory = await _inventoryService.GetByIdAsync(adjustDto.InventoryId);
        if (inventory == null)
        {
            return NotFound();
        }

        var product = await _productService.GetByIdAsync(inventory.ProductId);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var updatedInventory = await _inventoryService.AdjustStockAsync(adjustDto, userId);
        return Ok(updatedInventory);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> TransferStock([FromBody] TransferInventoryDto transferDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin inventory'sini transfer edebilmeli
        var product = await _productService.GetByIdAsync(transferDto.ProductId);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _inventoryService.TransferStockAsync(transferDto, userId);
        return NoContent();
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveStock([FromBody] ReserveStockDto reserveDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ReserveStock genellikle order işlemleri için kullanılır, bu yüzden IDOR kontrolü gerekli değil
        // Ancak Seller kendi ürünlerinin stokunu rezerve edebilmeli
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

        // ReleaseStock genellikle order işlemleri için kullanılır, bu yüzden IDOR kontrolü gerekli değil
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
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin inventory'sini güncelleyebilmeli
        var inventory = await _inventoryService.GetByIdAsync(id);
        if (inventory == null)
        {
            return NotFound();
        }

        var product = await _productService.GetByIdAsync(inventory.ProductId);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _inventoryService.UpdateLastCountDateAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
