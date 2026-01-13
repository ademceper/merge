using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;
using Merge.Application.Configuration;
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
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class StockMovementsController(
    IMediator mediator,
    IOptions<ShippingSettings> shippingSettings) : BaseController
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    /// <summary>
    /// Stok hareketi detaylarını getirir
    /// </summary>
    /// <param name="id">Stok hareketi ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Stok hareketi detayları</returns>
    /// <response code="200">Stok hareketi başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu stok hareketine erişim yetkisi yok</response>
    /// <response code="404">Stok hareketi bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
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
        var movement = await mediator.Send(query, cancellationToken);
        if (movement == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Seller sadece kendi ürünlerinin stock movement'larına erişebilmeli
        var productQuery = new GetProductByIdQuery(movement.ProductId);
        var product = await mediator.Send(productQuery, cancellationToken);
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
    /// <param name="inventoryId">Envanter ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Envantere ait stok hareketleri listesi</returns>
    /// <response code="200">Stok hareketleri başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu envanterin stok hareketlerine erişim yetkisi yok</response>
    /// <response code="404">Envanter veya ürün bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
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
        var inventory = await mediator.Send(inventoryQuery, cancellationToken);
        if (inventory == null)
        {
            return NotFound();
        }

        var productQuery = new GetProductByIdQuery(inventory.ProductId);
        var product = await mediator.Send(productQuery, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        var query = new GetStockMovementsByInventoryIdQuery(inventoryId);
        var movements = await mediator.Send(query, cancellationToken);
        return Ok(movements);
    }

    /// <summary>
    /// Ürün ID'sine göre stok hareketlerini getirir (pagination ile)
    /// </summary>
    /// <param name="productId">Ürün ID'si</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış stok hareketleri listesi</returns>
    /// <response code="200">Stok hareketleri başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu ürünün stok hareketlerine erişim yetkisi yok</response>
    /// <response code="404">Ürün bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
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
        var product = await mediator.Send(productQuery, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }

        if (product.SellerId.HasValue && product.SellerId.Value != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan
        if (pageSize > _shippingSettings.QueryLimits.MaxPageSize) 
            pageSize = _shippingSettings.QueryLimits.MaxPageSize;

        var query = new GetStockMovementsByProductIdQuery(productId, page, pageSize);
        var movements = await mediator.Send(query, cancellationToken);
        return Ok(movements);
    }

    /// <summary>
    /// Depo ID'sine göre stok hareketlerini getirir (pagination ile)
    /// </summary>
    /// <param name="warehouseId">Depo ID'si</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış stok hareketleri listesi</returns>
    /// <response code="200">Stok hareketleri başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="429">Çok fazla istek</response>
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
        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan
        if (pageSize > _shippingSettings.QueryLimits.MaxPageSize) 
            pageSize = _shippingSettings.QueryLimits.MaxPageSize;

        var query = new GetStockMovementsByWarehouseIdQuery(warehouseId, page, pageSize);
        var movements = await mediator.Send(query, cancellationToken);
        return Ok(movements);
    }

    /// <summary>
    /// Filtrelenmiş stok hareketlerini getirir
    /// </summary>
    /// <param name="filter">Stok hareketi filtreleme kriterleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Filtrelenmiş stok hareketleri listesi</returns>
    /// <response code="200">Stok hareketleri başarıyla getirildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu ürünün stok hareketlerine erişim yetkisi yok</response>
    /// <response code="404">Ürün bulunamadı (ProductId filtresi varsa)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("filter")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(IEnumerable<StockMovementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
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
            var product = await mediator.Send(productQuery, cancellationToken);
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
        var movements = await mediator.Send(query, cancellationToken);
        return Ok(movements);
    }

    /// <summary>
    /// Yeni stok hareketi oluşturur
    /// </summary>
    /// <param name="createDto">Stok hareketi oluşturma verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan stok hareketi bilgileri</returns>
    /// <response code="201">Stok hareketi başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu ürünün stok hareketini oluşturma yetkisi yok</response>
    /// <response code="404">Ürün veya envanter bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örn: stok miktarı negatif olur)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(StockMovementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
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
        var product = await mediator.Send(productQuery, cancellationToken);
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
        var movement = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = movement.Id }, movement);
    }
}
