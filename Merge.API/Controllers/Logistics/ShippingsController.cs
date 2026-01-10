using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Domain.Enums;
using Merge.API.Middleware;
using Merge.Application.Logistics.Queries.GetShippingById;
using Merge.Application.Logistics.Queries.GetShippingByOrderId;
using Merge.Application.Logistics.Queries.CalculateShippingCost;
using Merge.Application.Logistics.Queries.GetAvailableShippingProviders;
using Merge.Application.Logistics.Commands.CreateShipping;
using Merge.Application.Logistics.Commands.UpdateShippingTracking;
using Merge.Application.Logistics.Commands.UpdateShippingStatus;
using Merge.Application.Order.Queries.GetOrderById;

namespace Merge.API.Controllers.Logistics;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/logistics/shippings")]
[Authorize]
public class ShippingsController : BaseController
{
    private readonly IMediator _mediator;

    public ShippingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Mevcut kargo sağlayıcılarını getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kargo sağlayıcıları listesi</returns>
    /// <response code="200">Kargo sağlayıcıları başarıyla getirildi</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("providers")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ShippingProviderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ShippingProviderDto>>> GetProviders(
        CancellationToken cancellationToken = default)
    {
        var query = new GetAvailableShippingProvidersQuery();
        var providers = await _mediator.Send(query, cancellationToken);
        return Ok(providers);
    }

    /// <summary>
    /// Kargo detaylarını getirir
    /// </summary>
    /// <param name="id">Kargo ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kargo detayları</returns>
    /// <response code="200">Kargo başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu kargo bilgisine erişim yetkisi yok</response>
    /// <response code="404">Kargo bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi siparişlerinin kargo bilgilerine erişebilir
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var query = new GetShippingByIdQuery(id);
        var shipping = await _mediator.Send(query, cancellationToken);
        if (shipping == null)
        {
            return NotFound();
        }

        // Order ownership kontrolü
        var orderQuery = new GetOrderByIdQuery(shipping.OrderId);
        var order = await _mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(shipping);
    }

    /// <summary>
    /// Siparişe ait kargo bilgilerini getirir
    /// </summary>
    /// <param name="orderId">Sipariş ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Siparişe ait kargo bilgileri</returns>
    /// <response code="200">Kargo bilgileri başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu siparişin kargo bilgilerine erişim yetkisi yok</response>
    /// <response code="404">Sipariş veya kargo bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("order/{orderId}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> GetByOrderId(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi siparişlerinin kargo bilgilerine erişebilir
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var orderQuery = new GetOrderByIdQuery(orderId);
        var order = await _mediator.Send(orderQuery, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var query = new GetShippingByOrderIdQuery(orderId);
        var shipping = await _mediator.Send(query, cancellationToken);
        if (shipping == null)
        {
            return NotFound();
        }

        return Ok(shipping);
    }

    /// <summary>
    /// Kargo maliyetini hesaplar
    /// </summary>
    /// <param name="dto">Kargo maliyeti hesaplama verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Hesaplanan kargo maliyeti</returns>
    /// <response code="200">Kargo maliyeti başarıyla hesaplandı</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="404">Sipariş bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("calculate")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30 istek / dakika
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<decimal>> CalculateCost(
        [FromBody] CalculateShippingDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var query = new CalculateShippingCostQuery(dto.OrderId, dto.Provider);
        var cost = await _mediator.Send(query, cancellationToken);
        return Ok(new { cost });
    }

    /// <summary>
    /// Yeni kargo kaydı oluşturur (Admin only)
    /// </summary>
    /// <param name="dto">Kargo oluşturma verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan kargo bilgileri</returns>
    /// <response code="201">Kargo kaydı başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Sipariş bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örn: sipariş için zaten kargo kaydı var)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> CreateShipping(
        [FromBody] CreateShippingDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new CreateShippingCommand(dto.OrderId, dto.ShippingProvider, dto.ShippingCost);
        var shipping = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = shipping.Id }, shipping);
    }

    /// <summary>
    /// Kargo takip numarasını günceller (Admin only)
    /// </summary>
    /// <param name="shippingId">Kargo ID'si</param>
    /// <param name="dto">Takip numarası güncelleme verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenmiş kargo bilgileri</returns>
    /// <response code="200">Takip numarası başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Kargo kaydı bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPut("{shippingId}/tracking")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> UpdateTracking(
        Guid shippingId,
        [FromBody] UpdateTrackingDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new UpdateShippingTrackingCommand(shippingId, dto.TrackingNumber);
        var shipping = await _mediator.Send(command, cancellationToken);
        return Ok(shipping);
    }

    /// <summary>
    /// Kargo durumunu günceller (Admin only)
    /// </summary>
    /// <param name="shippingId">Kargo ID'si</param>
    /// <param name="dto">Durum güncelleme verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenmiş kargo bilgileri</returns>
    /// <response code="200">Kargo durumu başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi veya geçersiz durum</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Kargo kaydı bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örn: geçersiz durum geçişi)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPut("{shippingId}/status")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(ShippingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingDto>> UpdateStatus(
        Guid shippingId,
        [FromBody] UpdateShippingStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        if (!Enum.TryParse<ShippingStatus>(dto.Status, out var statusEnum))
        {
            return BadRequest("Geçersiz kargo durumu.");
        }

        var command = new UpdateShippingStatusCommand(shippingId, statusEnum);
        var shipping = await _mediator.Send(command, cancellationToken);
        return Ok(shipping);
    }
}

