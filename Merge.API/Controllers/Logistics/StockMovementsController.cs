using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Interfaces.Product;
using Merge.Application.Interfaces.Catalog;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;
using Merge.API.Middleware;

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

    /// <summary>
    /// Stok hareketi detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(StockMovementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StockMovementDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var movement = await _stockMovementService.GetByIdAsync(id, cancellationToken);
        if (movement == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi ürünlerinin stock movement'larına erişebilmeli
        var product = await _productService.GetByIdAsync(movement.ProductId, cancellationToken);
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

    /// <summary>
    /// Envanter ID'sine göre stok hareketlerini getirir
    /// </summary>
    [HttpGet("inventory/{inventoryId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<StockMovementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetByInventory(
        Guid inventoryId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Önce inventory'yi kontrol et
        var inventory = await _inventoryService.GetByIdAsync(inventoryId, cancellationToken);
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

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var movements = await _stockMovementService.GetByInventoryIdAsync(inventoryId, cancellationToken);
        return Ok(movements);
    }

    /// <summary>
    /// Ürün ID'sine göre stok hareketlerini getirir (pagination ile)
    /// </summary>
    [HttpGet("product/{productId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<StockMovementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<StockMovementDto>>> GetByProduct(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi ürünlerinin stock movement'larına erişebilmeli
        var product = await _productService.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var movements = await _stockMovementService.GetByProductIdAsync(productId, page, pageSize, cancellationToken);
        return Ok(movements);
    }

    /// <summary>
    /// Depo ID'sine göre stok hareketlerini getirir (pagination ile)
    /// </summary>
    [HttpGet("warehouse/{warehouseId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<StockMovementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<StockMovementDto>>> GetByWarehouse(
        Guid warehouseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        if (pageSize > 100) pageSize = 100; // Max limit

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var movements = await _stockMovementService.GetByWarehouseIdAsync(warehouseId, page, pageSize, cancellationToken);
        return Ok(movements);
    }

    /// <summary>
    /// Filtrelenmiş stok hareketlerini getirir
    /// </summary>
    [HttpPost("filter")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(IEnumerable<StockMovementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetFiltered(
        [FromBody] StockMovementFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi ürünlerinin stock movement'larına erişebilmeli
        // Eğer ProductId filtresi varsa kontrol et
        if (filter.ProductId.HasValue)
        {
            var product = await _productService.GetByIdAsync(filter.ProductId.Value, cancellationToken);
            if (product == null)
            {
                return NotFound();
            }

            if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var movements = await _stockMovementService.GetFilteredAsync(filter, cancellationToken);
        return Ok(movements);
    }

    /// <summary>
    /// Yeni stok hareketi oluşturur
    /// </summary>
    [HttpPost]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(StockMovementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StockMovementDto>> Create(
        [FromBody] CreateStockMovementDto createDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi ürünlerinin stock movement'larını oluşturabilmeli
        var product = await _productService.GetByIdAsync(createDto.ProductId, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var movement = await _stockMovementService.CreateAsync(createDto, userId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = movement.Id }, movement);
    }
}
