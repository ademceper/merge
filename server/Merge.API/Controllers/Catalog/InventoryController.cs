using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Application.DTOs.Catalog;
using Merge.Application.Common;
using Merge.API.Middleware;
using Merge.Application.Catalog.Queries.GetInventoryById;
using Merge.Application.Catalog.Queries.GetInventoriesByProductId;
using Merge.Application.Catalog.Queries.GetInventoriesByWarehouseId;
using Merge.Application.Catalog.Queries.GetInventoryByProductAndWarehouse;
using Merge.Application.Catalog.Queries.GetLowStockAlerts;
using Merge.Application.Catalog.Queries.GetStockReportByProduct;
using Merge.Application.Catalog.Queries.GetAvailableStock;
using Merge.Application.Catalog.Commands.CreateInventory;
using Merge.Application.Catalog.Commands.UpdateInventory;
using Merge.Application.Catalog.Commands.PatchInventory;
using Merge.Application.Catalog.Commands.DeleteInventory;
using Merge.Application.Catalog.Commands.AdjustStock;
using Merge.Application.Catalog.Commands.TransferStock;
using Merge.Application.Catalog.Commands.ReserveStock;
using Merge.Application.Catalog.Commands.ReleaseStock;
using Merge.Application.Catalog.Commands.UpdateLastCountDate;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.API.Controllers.Catalog;

// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/catalog/inventory")]
[Authorize(Roles = "Admin,Seller")]
public class InventoryController(
    IMediator mediator,
    IOptions<PaginationSettings> paginationSettings) : BaseController
{

    /// <summary>
    /// Envanter detaylarını getirir
    /// </summary>
    /// <param name="id">Envanter ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Envanter detayları</returns>
    /// <response code="200">Envanter başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">Envanter bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetInventoryByIdQuery(id, userId);
        var inventory = await mediator.Send(query, cancellationToken);
        
        if (inventory == null)
        {
            return NotFound();
        }

        return Ok(inventory);
    }

    /// <summary>
    /// Ürüne göre envanterleri getirir
    /// </summary>
    /// <param name="productId">Ürün ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Ürüne ait envanter listesi</returns>
    /// <response code="200">Envanterler başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">Ürün bulunamadı</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetInventoriesByProductIdQuery(productId, userId);
        var inventories = await mediator.Send(query, cancellationToken);
        
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
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var validPageSize = pageSize > paginationSettings.Value.MaxPageSize ? paginationSettings.Value.MaxPageSize : pageSize;
        var validPage = page < 1 ? 1 : page;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ SECURITY: PerformedBy userId'den alınmalı (IDOR protection)
        var query = new GetInventoriesByWarehouseIdQuery(warehouseId, userId, validPage, validPageSize);
        var inventories = await mediator.Send(query, cancellationToken);
        
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetInventoryByProductAndWarehouseQuery(productId, warehouseId, userId);
        var inventory = await mediator.Send(query, cancellationToken);
        
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
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var validPageSize = pageSize > paginationSettings.Value.MaxPageSize ? paginationSettings.Value.MaxPageSize : pageSize;
        var validPage = page < 1 ? 1 : page;

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ SECURITY: PerformedBy userId'den alınmalı (IDOR protection)
        var query = new GetLowStockAlertsQuery(userId, warehouseId, validPage, validPageSize);
        var alerts = await mediator.Send(query, cancellationToken);
        
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetStockReportByProductQuery(productId, userId);
        var report = await mediator.Send(query, cancellationToken);
        
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
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetAvailableStockQuery(productId, warehouseId, userId);
        var availableStock = await mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.3: Over-Posting Koruması - Anonymous object YASAK, DTO kullan
        return Ok(availableStock);
    }

    /// <summary>
    /// Yeni envanter kaydı oluşturur
    /// </summary>
    /// <param name="command">Envanter oluşturma komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan envanter</returns>
    /// <response code="201">Envanter başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">Ürün veya depo bulunamadı</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InventoryDto>> Create(
        [FromBody] CreateInventoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ SECURITY: PerformedBy userId'den alınmalı (IDOR protection)
        var updatedCommand = command with { PerformedBy = userId };
        var inventory = await mediator.Send(updatedCommand, cancellationToken);
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (CreateInventoryCommandHandler)

        return CreatedAtAction(nameof(GetById), new { id = inventory.Id }, inventory);
    }

    /// <summary>
    /// Envanter kaydını günceller
    /// </summary>
    /// <param name="id">Envanter ID</param>
    /// <param name="command">Envanter güncelleme komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenen envanter</returns>
    /// <response code="200">Envanter başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">Envanter bulunamadı</response>
    /// <response code="422">İş kuralı ihlali veya concurrency conflict</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPut("{id}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InventoryDto>> Update(
        Guid id,
        [FromBody] UpdateInventoryCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ SECURITY: PerformedBy userId'den alınmalı (IDOR protection)
        var updatedCommand = command with { Id = id, PerformedBy = userId };
        var updatedInventory = await mediator.Send(updatedCommand, cancellationToken);
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (UpdateInventoryCommandHandler)

        return Ok(updatedInventory);
    }

    /// <summary>
    /// Envanteri kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InventoryDto>> Patch(
        Guid id,
        [FromBody] PatchInventoryDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var command = new PatchInventoryCommand(id, patchDto, userId);
        var inventory = await mediator.Send(command, cancellationToken);
        return Ok(inventory);
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new DeleteInventoryCommand(id, userId);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (DeleteInventoryCommandHandler)

        return NoContent();
    }

    /// <summary>
    /// Stok miktarını ayarlar
    /// </summary>
    /// <param name="command">Stok ayarlama komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenen envanter</returns>
    /// <response code="200">Stok başarıyla ayarlandı</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">Envanter bulunamadı</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("adjust")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(InventoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<InventoryDto>> AdjustStock(
        [FromBody] AdjustStockCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ SECURITY: PerformedBy userId'den alınmalı (IDOR protection)
        var updatedCommand = command with { PerformedBy = userId };
        var updatedInventory = await mediator.Send(updatedCommand, cancellationToken);
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (AdjustStockCommandHandler)

        return Ok(updatedInventory);
    }

    /// <summary>
    /// Stok transferi yapar
    /// </summary>
    /// <param name="command">Stok transfer komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Stok transferi başarıyla tamamlandı</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">Envanter bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (yetersiz stok)</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("transfer")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> TransferStock(
        [FromBody] TransferStockCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ SECURITY: PerformedBy userId'den alınmalı (IDOR protection)
        var updatedCommand = command with { PerformedBy = userId };
        var result = await mediator.Send(updatedCommand, cancellationToken);
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (TransferStockCommandHandler)

        return NoContent();
    }

    /// <summary>
    /// Stok rezervasyonu yapar
    /// </summary>
    /// <param name="command">Stok rezervasyon komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Stok rezervasyonu başarıyla tamamlandı</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">Envanter bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (yetersiz stok)</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("reserve")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (order işlemleri için yüksek limit)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ReserveStock(
        [FromBody] ReserveStockCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ SECURITY: PerformedBy userId'den alınmalı (IDOR protection)
        var updatedCommand = command with { PerformedBy = userId };
        var result = await mediator.Send(updatedCommand, cancellationToken);
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (ReserveStockCommandHandler)

        return NoContent();
    }

    /// <summary>
    /// Stok rezervasyonunu iptal eder
    /// </summary>
    /// <param name="command">Stok serbest bırakma komutu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem sonucu</returns>
    /// <response code="204">Stok rezervasyonu başarıyla iptal edildi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması yapılmamış</response>
    /// <response code="403">Kullanıcının bu işlem için yetkisi yok</response>
    /// <response code="404">Envanter bulunamadı</response>
    /// <response code="422">İş kuralı ihlali</response>
    /// <response code="429">Çok fazla istek - Rate limit aşıldı</response>
    [HttpPost("release")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika (order işlemleri için yüksek limit)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ReleaseStock(
        [FromBody] ReleaseStockCommand command,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ SECURITY: PerformedBy userId'den alınmalı (IDOR protection)
        var updatedCommand = command with { PerformedBy = userId };
        var result = await mediator.Send(updatedCommand, cancellationToken);
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (ReleaseStockCommandHandler)

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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new UpdateLastCountDateCommand(id, userId);
        var result = await mediator.Send(command, cancellationToken);
        
        if (!result)
        {
            return NotFound();
        }
        
        // ✅ BOLUM 3.2: IDOR Korumasi - Handler seviyesinde yapılıyor (UpdateLastCountDateCommandHandler)

        return NoContent();
    }
}
