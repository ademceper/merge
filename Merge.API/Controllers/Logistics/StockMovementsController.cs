using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Logistics.Queries.GetStockMovementById;
using Merge.Application.Logistics.Queries.GetStockMovementsByInventoryId;
using Merge.Application.Logistics.Queries.GetStockMovementsByProductId;
using Merge.Application.Logistics.Queries.GetStockMovementsByWarehouseId;
using Merge.Application.Logistics.Queries.GetFilteredStockMovements;
using Merge.Application.Logistics.Commands.CreateStockMovement;
using Merge.Application.Product.Queries.GetProductById;
using Merge.Application.Catalog.Queries.GetInventoryById;

namespace Merge.API.Controllers.Logistics;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/logistics/stock-movements")]
[Authorize(Roles = "Admin,Seller")]
public class StockMovementsController : BaseController
{
    private readonly IMediator _mediator;

    public StockMovementsController(IMediator mediator)
    {
        _mediator = mediator;
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

        var query = new GetStockMovementByIdQuery(id);
        var movement = await _mediator.Send(query, cancellationToken);
        if (movement == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi ürünlerinin stock movement'larına erişebilmeli
        var productQuery = new GetProductByIdQuery(movement.ProductId);
        var product = await _mediator.Send(productQuery, cancellationToken);
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
        var inventoryQuery = new GetInventoryByIdQuery(inventoryId);
        var inventory = await _mediator.Send(inventoryQuery, cancellationToken);
        if (inventory == null)
        {
            return NotFound();
        }

        var productQuery = new GetProductByIdQuery(inventory.ProductId);
        var product = await _mediator.Send(productQuery, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var query = new GetStockMovementsByInventoryIdQuery(inventoryId);
        var movements = await _mediator.Send(query, cancellationToken);
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
        var productQuery = new GetProductByIdQuery(productId);
        var product = await _mediator.Send(productQuery, cancellationToken);
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

        var query = new GetStockMovementsByProductIdQuery(productId, page, pageSize);
        var movements = await _mediator.Send(query, cancellationToken);
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

        var query = new GetStockMovementsByWarehouseIdQuery(warehouseId, page, pageSize);
        var movements = await _mediator.Send(query, cancellationToken);
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
            var productQuery = new GetProductByIdQuery(filter.ProductId.Value);
            var product = await _mediator.Send(productQuery, cancellationToken);
            if (product == null)
            {
                return NotFound();
            }

            if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
        }

        var query = new GetFilteredStockMovementsQuery(
            filter.ProductId,
            filter.WarehouseId,
            filter.MovementType,
            filter.StartDate,
            filter.EndDate,
            filter.Page,
            filter.PageSize);
        var movements = await _mediator.Send(query, cancellationToken);
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
        var productQuery = new GetProductByIdQuery(createDto.ProductId);
        var product = await _mediator.Send(productQuery, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var command = new CreateStockMovementCommand(
            createDto.ProductId,
            createDto.WarehouseId,
            createDto.MovementType,
            createDto.Quantity,
            createDto.ReferenceNumber,
            createDto.ReferenceId,
            createDto.Notes,
            createDto.FromWarehouseId,
            createDto.ToWarehouseId,
            userId);
        var movement = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = movement.Id }, movement);
    }
}
