using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.API.Middleware;
using Merge.Application.Logistics.Commands.CreatePickPack;
using Merge.Application.Logistics.Queries.GetPickPackById;
using Merge.Application.Logistics.Queries.GetPickPackByPackNumber;
using Merge.Application.Logistics.Queries.GetPickPacksByOrderId;
using Merge.Application.Logistics.Queries.GetAllPickPacks;
using Merge.Application.Logistics.Queries.GetPickPackStats;
using Merge.Application.Logistics.Commands.UpdatePickPackDetails;
using Merge.Application.Logistics.Commands.StartPicking;
using Merge.Application.Logistics.Commands.CompletePicking;
using Merge.Application.Logistics.Commands.StartPacking;
using Merge.Application.Logistics.Commands.CompletePacking;
using Merge.Application.Logistics.Commands.MarkPickPackAsShipped;
using Merge.Application.Logistics.Commands.UpdatePickPackItemStatus;
using Merge.Application.Order.Queries.GetOrderById;
using Merge.Domain.Enums;

namespace Merge.API.Controllers.Logistics;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/logistics/pick-packs")]
[Authorize(Roles = "Admin,Manager,Warehouse")]
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class PickPacksController(
    IMediator mediator,
    IOptions<ShippingSettings> shippingSettings) : BaseController
{
    private readonly ShippingSettings _shippingSettings = shippingSettings.Value;

    /// <summary>
    /// Yeni pick-pack kaydı oluşturur
    /// </summary>
    /// <param name="dto">Pick-pack oluşturma verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan pick-pack bilgileri</returns>
    /// <response code="201">Pick-pack başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Sipariş veya depo bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örn: sipariş için zaten pick-pack mevcut)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(PickPackDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PickPackDto>> CreatePickPack(
        [FromBody] CreatePickPackDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new CreatePickPackCommand(dto.OrderId, dto.WarehouseId, dto.Notes);
        var pickPack = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPickPack), new { id = pickPack.Id }, pickPack);
    }

    /// <summary>
    /// Pick-pack detaylarını getirir
    /// </summary>
    /// <param name="id">Pick-pack ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Pick-pack detayları</returns>
    /// <response code="200">Pick-pack başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu pick-pack'e erişim yetkisi yok</response>
    /// <response code="404">Pick-pack bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PickPackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PickPackDto>> GetPickPack(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var query = new GetPickPackByIdQuery(id);
        var pickPack = await mediator.Send(query, cancellationToken);
        if (pickPack == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi siparişlerinin pick-pack'lerine erişebilmeli
        var orderQuery = new GetOrderByIdQuery(pickPack.OrderId);
        var order = await mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Warehouse"))
        {
            return Forbid();
        }

        return Ok(pickPack);
    }

    /// <summary>
    /// Paket numarasına göre pick-pack getirir
    /// </summary>
    /// <param name="packNumber">Paket numarası (örn: PK-20260105-000001)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Pick-pack detayları</returns>
    /// <response code="200">Pick-pack başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu pick-pack'e erişim yetkisi yok</response>
    /// <response code="404">Pick-pack bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("pack-number/{packNumber}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PickPackDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PickPackDto>> GetPickPackByPackNumber(
        string packNumber,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var query = new GetPickPackByPackNumberQuery(packNumber);
        var pickPack = await mediator.Send(query, cancellationToken);
        if (pickPack == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi siparişlerinin pick-pack'lerine erişebilmeli
        var orderQuery = new GetOrderByIdQuery(pickPack.OrderId);
        var order = await mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Warehouse"))
        {
            return Forbid();
        }

        return Ok(pickPack);
    }

    /// <summary>
    /// Siparişe ait pick-pack'leri getirir
    /// </summary>
    /// <param name="orderId">Sipariş ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Siparişe ait pick-pack listesi</returns>
    /// <response code="200">Pick-pack'ler başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu siparişin pick-pack'lerine erişim yetkisi yok</response>
    /// <response code="404">Sipariş bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("order/{orderId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<PickPackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<PickPackDto>>> GetPickPacksByOrder(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi siparişlerinin pick-pack'lerine erişebilmeli
        var orderQuery = new GetOrderByIdQuery(orderId);
        var order = await mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Warehouse"))
        {
            return Forbid();
        }

        var query = new GetPickPacksByOrderIdQuery(orderId);
        var pickPacks = await mediator.Send(query, cancellationToken);
        return Ok(pickPacks);
    }

    /// <summary>
    /// Tüm pick-pack'leri getirir (pagination ile)
    /// </summary>
    /// <param name="status">Durum filtresi (opsiyonel)</param>
    /// <param name="warehouseId">Depo ID'si (opsiyonel filtre)</param>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 20, maksimum: 100)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Sayfalanmış pick-pack listesi</returns>
    /// <response code="200">Pick-pack'ler başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(PagedResult<PickPackDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<PickPackDto>>> GetAllPickPacks(
        [FromQuery] string? status = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan
        if (pageSize > _shippingSettings.QueryLimits.MaxPageSize) 
            pageSize = _shippingSettings.QueryLimits.MaxPageSize;

        PickPackStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PickPackStatus>(status, out var parsedStatus))
        {
            statusEnum = parsedStatus;
        }

        var query = new GetAllPickPacksQuery(statusEnum, warehouseId, page, pageSize);
        var pickPacks = await mediator.Send(query, cancellationToken);
        return Ok(pickPacks);
    }

    /// <summary>
    /// Pick-pack detaylarını günceller (notlar, ağırlık, boyutlar, paket sayısı)
    /// NOT: Status transition'ları için ayrı endpoint'ler kullanılmalı (start-picking, complete-picking, start-packing, complete-packing, mark-shipped)
    /// </summary>
    /// <param name="id">Pick-pack ID'si</param>
    /// <param name="dto">Detay güncelleme verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Pick-pack detayları başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Pick-pack bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPut("{id}/details")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateDetails(
        Guid id,
        [FromBody] UpdatePickPackStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 1.1: Rich Domain Model - Status transition'ları için ayrı endpoint'ler kullanılmalı
        // Bu endpoint sadece details (notes, weight, dimensions, packageCount) update için kullanılır
        var command = new UpdatePickPackDetailsCommand(id, dto.Notes, dto.Weight, dto.Dimensions, dto.PackageCount);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pick işlemini başlatır
    /// </summary>
    /// <param name="id">Pick-pack ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Pick işlemi başarıyla başlatıldı</response>
    /// <response code="400">Geçersiz istek (örn: pick-pack zaten başka bir durumda)</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Pick-pack bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örn: geçersiz durum geçişi)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{id}/start-picking")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> StartPicking(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new StartPickingCommand(id, userId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pick işlemini tamamlar
    /// </summary>
    /// <param name="id">Pick-pack ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Pick işlemi başarıyla tamamlandı</response>
    /// <response code="400">Geçersiz istek (örn: pick-pack picking durumunda değil)</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Pick-pack bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örn: geçersiz durum geçişi)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{id}/complete-picking")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CompletePicking(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new CompletePickingCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pack işlemini başlatır
    /// </summary>
    /// <param name="id">Pick-pack ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Pack işlemi başarıyla başlatıldı</response>
    /// <response code="400">Geçersiz istek (örn: pick-pack picked durumunda değil)</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Pick-pack bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örn: geçersiz durum geçişi)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{id}/start-packing")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> StartPacking(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new StartPackingCommand(id, userId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pack işlemini tamamlar
    /// </summary>
    /// <param name="id">Pick-pack ID'si</param>
    /// <param name="dto">Paketleme tamamlama verileri (ağırlık, boyutlar, paket sayısı)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Pack işlemi başarıyla tamamlandı</response>
    /// <response code="400">Geçersiz istek verisi veya pick-pack packing durumunda değil</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Pick-pack bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örn: tüm kalemler paketlenmemiş, geçersiz durum geçişi)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{id}/complete-packing")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CompletePacking(
        Guid id,
        [FromBody] CompletePackingDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new CompletePackingCommand(id, dto.Weight, dto.Dimensions, dto.PackageCount);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pick-pack'i kargoya verildi olarak işaretler
    /// </summary>
    /// <param name="id">Pick-pack ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Pick-pack kargoya verildi olarak işaretlendi</response>
    /// <response code="400">Geçersiz istek (örn: pick-pack packed durumunda değil)</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Pick-pack bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örn: geçersiz durum geçişi)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{id}/mark-shipped")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> MarkAsShipped(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new MarkPickPackAsShippedCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pick-pack item durumunu günceller
    /// </summary>
    /// <param name="itemId">Pick-pack item ID'si</param>
    /// <param name="dto">Item durum güncelleme verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Pick-pack item durumu başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Pick-pack item bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPut("items/{itemId}/status")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateItemStatus(
        Guid itemId,
        [FromBody] PickPackItemStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new UpdatePickPackItemStatusCommand(itemId, dto.IsPicked, dto.IsPacked, dto.Location);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Pick-pack istatistiklerini getirir
    /// </summary>
    /// <param name="warehouseId">Depo ID'si (opsiyonel filtre)</param>
    /// <param name="startDate">Başlangıç tarihi (opsiyonel filtre)</param>
    /// <param name="endDate">Bitiş tarihi (opsiyonel filtre)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Pick-pack istatistikleri (durum bazında sayılar)</returns>
    /// <response code="200">Pick-pack istatistikleri başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="429">Çok fazla istek</response>
    /// <remarks>
    /// NOTE: Dictionary&lt;string, int&gt; burada kabul edilebilir çünkü stats için key-value çiftleri dinamik
    /// </remarks>
    [HttpGet("stats")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<Dictionary<string, int>>> GetStats(
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPickPackStatsQuery(warehouseId, startDate, endDate);
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}

