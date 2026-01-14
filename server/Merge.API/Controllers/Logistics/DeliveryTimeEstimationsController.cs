using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.API.Middleware;
using Merge.Application.Logistics.Queries.EstimateDeliveryTime;
using Merge.Application.Logistics.Queries.GetAllDeliveryTimeEstimations;
using Merge.Application.Logistics.Queries.GetDeliveryTimeEstimationById;
using Merge.Application.Logistics.Commands.CreateDeliveryTimeEstimation;
using Merge.Application.Logistics.Commands.UpdateDeliveryTimeEstimation;
using Merge.Application.Logistics.Commands.DeleteDeliveryTimeEstimation;

namespace Merge.API.Controllers.Logistics;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/logistics/delivery-time")]
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class DeliveryTimeEstimationsController(IMediator mediator) : BaseController
{

    /// <summary>
    /// Teslimat süresini tahmin eder
    /// </summary>
    /// <param name="dto">Teslimat süresi tahmin parametreleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Tahmin edilen teslimat süresi bilgileri</returns>
    /// <response code="200">Teslimat süresi başarıyla tahmin edildi</response>
    /// <response code="400">Geçersiz istek parametreleri</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("estimate")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(DeliveryTimeEstimateResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DeliveryTimeEstimateResultDto>> EstimateDeliveryTime(
        [FromQuery] EstimateDeliveryTimeDto dto,
        CancellationToken cancellationToken = default)
    {
        var query = new EstimateDeliveryTimeQuery(
            dto.ProductId,
            dto.CategoryId,
            dto.WarehouseId,
            dto.ShippingProviderId,
            dto.City,
            dto.Country,
            dto.OrderDate);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tüm teslimat süresi tahminlerini getirir
    /// </summary>
    /// <param name="productId">Ürün ID'si (opsiyonel filtre)</param>
    /// <param name="categoryId">Kategori ID'si (opsiyonel filtre)</param>
    /// <param name="warehouseId">Depo ID'si (opsiyonel filtre)</param>
    /// <param name="isActive">Sadece aktif tahminleri getir (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Teslimat süresi tahminleri listesi</returns>
    /// <response code="200">Teslimat süresi tahminleri başarıyla getirildi</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<DeliveryTimeEstimationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<DeliveryTimeEstimationDto>>> GetAllEstimations(
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllDeliveryTimeEstimationsQuery(productId, categoryId, warehouseId, isActive);
        var estimations = await mediator.Send(query, cancellationToken);
        return Ok(estimations);
    }

    /// <summary>
    /// Teslimat süresi tahmini detaylarını getirir
    /// </summary>
    /// <param name="id">Teslimat süresi tahmini ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Teslimat süresi tahmini detayları</returns>
    /// <response code="200">Teslimat süresi tahmini başarıyla getirildi</response>
    /// <response code="404">Teslimat süresi tahmini bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(DeliveryTimeEstimationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DeliveryTimeEstimationDto>> GetEstimation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDeliveryTimeEstimationByIdQuery(id);
        var estimation = await mediator.Send(query, cancellationToken);
        if (estimation == null)
        {
            return NotFound();
        }
        return Ok(estimation);
    }

    /// <summary>
    /// Yeni teslimat süresi tahmini oluşturur (Admin, Manager)
    /// </summary>
    /// <param name="dto">Teslimat süresi tahmini oluşturma verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan teslimat süresi tahmini bilgileri</returns>
    /// <response code="201">Teslimat süresi tahmini başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="422">İş kuralı ihlali (örn: minDays > maxDays)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(DeliveryTimeEstimationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<DeliveryTimeEstimationDto>> CreateEstimation(
        [FromBody] CreateDeliveryTimeEstimationDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new CreateDeliveryTimeEstimationCommand(
            dto.ProductId,
            dto.CategoryId,
            dto.WarehouseId,
            dto.ShippingProviderId,
            dto.City,
            dto.Country,
            dto.MinDays,
            dto.MaxDays,
            dto.AverageDays,
            dto.IsActive,
            dto.Conditions);
        var estimation = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetEstimation), new { id = estimation.Id }, estimation);
    }

    /// <summary>
    /// Teslimat süresi tahminini günceller (Admin, Manager)
    /// </summary>
    /// <param name="id">Teslimat süresi tahmini ID'si</param>
    /// <param name="dto">Güncelleme verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Teslimat süresi tahmini başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Teslimat süresi tahmini bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örn: minDays > maxDays)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateEstimation(
        Guid id,
        [FromBody] UpdateDeliveryTimeEstimationDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new UpdateDeliveryTimeEstimationCommand(
            id,
            dto.MinDays,
            dto.MaxDays,
            dto.AverageDays,
            dto.IsActive,
            dto.Conditions);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Teslimat süresi tahminini siler (Admin, Manager)
    /// </summary>
    /// <param name="id">Teslimat süresi tahmini ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Teslimat süresi tahmini başarıyla silindi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Teslimat süresi tahmini bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteEstimation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteDeliveryTimeEstimationCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

