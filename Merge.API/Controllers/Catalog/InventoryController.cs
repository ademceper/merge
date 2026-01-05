using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Catalog;
using Merge.Application.Interfaces.Product;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;
using Merge.API.Middleware;


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

    /// <summary>
    /// Envanter detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InventoryDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var inventory = await _inventoryService.GetByIdAsync(id, cancellationToken);
        if (inventory == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sine erişebilmeli
        var product = await _productService.GetByIdAsync(inventory.ProductId, cancellationToken);
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

    /// <summary>
    /// Ürüne göre envanterleri getirir
    /// </summary>
    [HttpGet("product/{productId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<InventoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<InventoryDto>>> GetByProduct(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sine erişebilmeli
        var product = await _productService.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var inventories = await _inventoryService.GetByProductIdAsync(productId, cancellationToken);
        return Ok(inventories);
    }

    /// <summary>
    /// Depoya göre envanterleri getirir
    /// </summary>
    [HttpGet("warehouse/{warehouseId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<InventoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<InventoryDto>>> GetByWarehouse(
        Guid warehouseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // Admin tüm warehouse'ları görebilir, Seller sadece kendi ürünlerinin inventory'sini görebilir
        // Bu endpoint'te filtreleme service katmanında yapılmalı
        var inventories = await _inventoryService.GetByWarehouseIdAsync(warehouseId, page, pageSize, cancellationToken);
        return Ok(inventories);
    }

    /// <summary>
    /// Ürün ve depoya göre envanteri getirir
    /// </summary>
    [HttpGet("product/{productId}/warehouse/{warehouseId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InventoryDto>> GetByProductAndWarehouse(
        Guid productId,
        Guid warehouseId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sine erişebilmeli
        var product = await _productService.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var inventory = await _inventoryService.GetByProductAndWarehouseAsync(productId, warehouseId, cancellationToken);
        if (inventory == null)
        {
            return NotFound();
        }
        return Ok(inventory);
    }

    /// <summary>
    /// Düşük stok uyarılarını getirir
    /// </summary>
    [HttpGet("low-stock")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(PagedResult<LowStockAlertDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<LowStockAlertDto>>> GetLowStockAlerts(
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var alerts = await _inventoryService.GetLowStockAlertsAsync(warehouseId, page, pageSize, cancellationToken);
        return Ok(alerts);
    }

    /// <summary>
    /// Ürün stok raporunu getirir
    /// </summary>
    [HttpGet("report/product/{productId}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(StockReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StockReportDto>> GetStockReport(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin stock report'una erişebilmeli
        var product = await _productService.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var report = await _inventoryService.GetStockReportByProductAsync(productId, cancellationToken);
        if (report == null)
        {
            return NotFound();
        }
        return Ok(report);
    }

    /// <summary>
    /// Ürünün mevcut stok miktarını getirir
    /// </summary>
    [HttpGet("available/{productId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(AvailableStockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AvailableStockDto>> GetAvailableStock(
        Guid productId,
        [FromQuery] Guid? warehouseId = null,
        CancellationToken cancellationToken = default)
    {
        // Available stock bilgisi public olabilir (ürün detay sayfasında gösterilir)
        // Ancak Seller kendi ürünlerinin stokunu görebilmeli
        var availableStock = await _inventoryService.GetAvailableStockAsync(productId, warehouseId, cancellationToken);
        
        // ✅ BOLUM 4.3: Over-Posting Koruması - Anonymous object YASAK, DTO kullan
        return Ok(new AvailableStockDto
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            AvailableStock = availableStock
        });
    }

    /// <summary>
    /// Yeni envanter kaydı oluşturur
    /// </summary>
    [HttpPost]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InventoryDto>> Create(
        [FromBody] CreateInventoryDto createDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sini oluşturabilmeli
        var product = await _productService.GetByIdAsync(createDto.ProductId, cancellationToken);
        if (product == null)
        {
            return NotFound("Ürün bulunamadı.");
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var inventory = await _inventoryService.CreateAsync(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = inventory.Id }, inventory);
    }

    /// <summary>
    /// Envanter kaydını günceller
    /// </summary>
    [HttpPut("{id}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InventoryDto>> Update(
        Guid id,
        [FromBody] UpdateInventoryDto updateDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sini güncelleyebilmeli
        var inventory = await _inventoryService.GetByIdAsync(id, cancellationToken);
        if (inventory == null)
        {
            return NotFound();
        }

        var product = await _productService.GetByIdAsync(inventory.ProductId, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var updatedInventory = await _inventoryService.UpdateAsync(id, updateDto, cancellationToken);
        if (updatedInventory == null)
        {
            return NotFound();
        }
        return Ok(updatedInventory);
    }

    /// <summary>
    /// Envanter kaydını siler
    /// </summary>
    [HttpDelete("{id}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sini silebilmeli
        var inventory = await _inventoryService.GetByIdAsync(id, cancellationToken);
        if (inventory == null)
        {
            return NotFound();
        }

        var product = await _productService.GetByIdAsync(inventory.ProductId, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _inventoryService.DeleteAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Stok miktarını ayarlar
    /// </summary>
    [HttpPost("adjust")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InventoryDto>> AdjustStock(
        [FromBody] AdjustInventoryDto adjustDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sini güncelleyebilmeli
        var inventory = await _inventoryService.GetByIdAsync(adjustDto.InventoryId, cancellationToken);
        if (inventory == null)
        {
            return NotFound();
        }

        var product = await _productService.GetByIdAsync(inventory.ProductId, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var updatedInventory = await _inventoryService.AdjustStockAsync(adjustDto, userId, cancellationToken);
        return Ok(updatedInventory);
    }

    /// <summary>
    /// Stok transferi yapar
    /// </summary>
    [HttpPost("transfer")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> TransferStock(
        [FromBody] TransferInventoryDto transferDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sini transfer edebilmeli
        var product = await _productService.GetByIdAsync(transferDto.ProductId, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _inventoryService.TransferStockAsync(transferDto, userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Stok rezervasyonu yapar
    /// </summary>
    [HttpPost("reserve")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (order işlemleri için yüksek limit)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ReserveStock(
        [FromBody] ReserveStockDto reserveDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin stokunu rezerve edebilmeli
        var product = await _productService.GetByIdAsync(reserveDto.ProductId, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _inventoryService.ReserveStockAsync(
            reserveDto.ProductId,
            reserveDto.WarehouseId,
            reserveDto.Quantity,
            reserveDto.OrderId,
            cancellationToken
        );
        return NoContent();
    }

    /// <summary>
    /// Stok rezervasyonunu iptal eder
    /// </summary>
    [HttpPost("release")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (order işlemleri için yüksek limit)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ReleaseStock(
        [FromBody] ReleaseStockDto releaseDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin stokunu serbest bırakabilmeli
        var product = await _productService.GetByIdAsync(releaseDto.ProductId, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _inventoryService.ReleaseStockAsync(
            releaseDto.ProductId,
            releaseDto.WarehouseId,
            releaseDto.Quantity,
            releaseDto.OrderId,
            cancellationToken
        );
        return NoContent();
    }

    /// <summary>
    /// Envanter sayım tarihini günceller
    /// </summary>
    [HttpPost("{id}/update-count-date")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateLastCountDate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Korumasi - Seller sadece kendi ürünlerinin inventory'sini güncelleyebilmeli
        var inventory = await _inventoryService.GetByIdAsync(id, cancellationToken);
        if (inventory == null)
        {
            return NotFound();
        }

        var product = await _productService.GetByIdAsync(inventory.ProductId, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var result = await _inventoryService.UpdateLastCountDateAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}
