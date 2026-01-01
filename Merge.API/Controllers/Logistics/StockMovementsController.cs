using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Interfaces.Product;
using Merge.Application.Interfaces.Catalog;
using Merge.Application.DTOs.Logistics;


namespace Merge.API.Controllers.Logistics;

[ApiController]
[Route("api/logistics/stock-movements")]
[Authorize(Roles = "Admin,Seller")]
public class StockMovementsController : BaseController
{
    private readonly IStockMovementService _stockMovementService;
    private readonly IProductService _productService;
    private readonly IInventoryService _inventoryService;

    public StockMovementsController(
        IStockMovementService stockMovementService,
        IProductService productService,
        IInventoryService inventoryService)
    {
        _stockMovementService = stockMovementService;
        _productService = productService;
        _inventoryService = inventoryService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StockMovementDto>> GetById(Guid id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var movement = await _stockMovementService.GetByIdAsync(id);
        if (movement == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin stock movement'larına erişebilmeli
        var product = await _productService.GetByIdAsync(movement.ProductId);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return Ok(movement);
    }

    [HttpGet("inventory/{inventoryId}")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetByInventory(Guid inventoryId)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Önce inventory'yi kontrol et
        var inventory = await _inventoryService.GetByIdAsync(inventoryId);
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

        var movements = await _stockMovementService.GetByInventoryIdAsync(inventoryId);
        return Ok(movements);
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetByProduct(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin stock movement'larına erişebilmeli
        var product = await _productService.GetByIdAsync(productId);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var movements = await _stockMovementService.GetByProductIdAsync(productId, page, pageSize);
        return Ok(movements);
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetByWarehouse(
        Guid warehouseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var movements = await _stockMovementService.GetByWarehouseIdAsync(warehouseId, page, pageSize);
        return Ok(movements);
    }

    [HttpPost("filter")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetFiltered([FromBody] StockMovementFilterDto filter)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin stock movement'larına erişebilmeli
        // Eğer ProductId filtresi varsa kontrol et
        if (filter.ProductId.HasValue)
        {
            var product = await _productService.GetByIdAsync(filter.ProductId.Value);
            if (product == null)
            {
                return NotFound();
            }

            if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
        }

        var movements = await _stockMovementService.GetFilteredAsync(filter);
        return Ok(movements);
    }

    [HttpPost]
    public async Task<ActionResult<StockMovementDto>> Create([FromBody] CreateStockMovementDto createDto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ SECURITY: IDOR koruması - Seller sadece kendi ürünlerinin stock movement'larını oluşturabilmeli
        var product = await _productService.GetByIdAsync(createDto.ProductId);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var movement = await _stockMovementService.CreateAsync(createDto, userId);
        return CreatedAtAction(nameof(GetById), new { id = movement.Id }, movement);
    }
}
