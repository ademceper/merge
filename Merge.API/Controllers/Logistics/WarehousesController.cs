using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.API.Middleware;
using Merge.Application.Logistics.Queries.GetAllWarehouses;
using Merge.Application.Logistics.Queries.GetActiveWarehouses;
using Merge.Application.Logistics.Queries.GetWarehouseById;
using Merge.Application.Logistics.Queries.GetWarehouseByCode;
using Merge.Application.Logistics.Commands.CreateWarehouse;
using Merge.Application.Logistics.Commands.UpdateWarehouse;
using Merge.Application.Logistics.Commands.DeleteWarehouse;
using Merge.Application.Logistics.Commands.ActivateWarehouse;
using Merge.Application.Logistics.Commands.DeactivateWarehouse;

namespace Merge.API.Controllers.Logistics;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/logistics/warehouses")]
[Authorize(Roles = "Admin")]
public class WarehousesController : BaseController
{
    private readonly IMediator _mediator;

    public WarehousesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Tüm depoları getirir (Admin only)
    /// </summary>
    /// <param name="includeInactive">Pasif depoları da dahil et (varsayılan: false)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Depo listesi</returns>
    /// <response code="200">Depolar başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllWarehousesQuery(includeInactive);
        var warehouses = await _mediator.Send(query, cancellationToken);
        return Ok(warehouses);
    }

    /// <summary>
    /// Aktif depoları getirir (Admin only)
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Aktif depo listesi</returns>
    /// <response code="200">Aktif depolar başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("active")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetActive(
        CancellationToken cancellationToken = default)
    {
        var query = new GetActiveWarehousesQuery();
        var warehouses = await _mediator.Send(query, cancellationToken);
        return Ok(warehouses);
    }

    /// <summary>
    /// Depo detaylarını getirir (Admin only)
    /// </summary>
    /// <param name="id">Depo ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Depo detayları</returns>
    /// <response code="200">Depo başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Depo bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<WarehouseDto>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetWarehouseByIdQuery(id);
        var warehouse = await _mediator.Send(query, cancellationToken);
        if (warehouse == null)
        {
            return NotFound();
        }
        return Ok(warehouse);
    }

    /// <summary>
    /// Depo koduna göre getirir (Admin only)
    /// </summary>
    /// <param name="code">Depo kodu (örn: WH001)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Depo detayları</returns>
    /// <response code="200">Depo başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Depo bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("code/{code}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<WarehouseDto>> GetByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var query = new GetWarehouseByCodeQuery(code);
        var warehouse = await _mediator.Send(query, cancellationToken);
        if (warehouse == null)
        {
            return NotFound();
        }
        return Ok(warehouse);
    }

    /// <summary>
    /// Yeni depo oluşturur (Admin only)
    /// </summary>
    /// <param name="createDto">Depo oluşturma verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan depo bilgileri</returns>
    /// <response code="201">Depo başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="422">İş kuralı ihlali (örn: aynı kod ile depo mevcut)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<WarehouseDto>> Create(
        [FromBody] CreateWarehouseDto createDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var command = new CreateWarehouseCommand(
            createDto.Name,
            createDto.Code,
            createDto.Address,
            createDto.City,
            createDto.Country,
            createDto.PostalCode,
            createDto.ContactPerson,
            createDto.ContactPhone,
            createDto.ContactEmail,
            createDto.Capacity,
            createDto.Description);
        var warehouse = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
    }

    /// <summary>
    /// Depo bilgilerini günceller (Admin only)
    /// </summary>
    /// <param name="id">Depo ID'si</param>
    /// <param name="updateDto">Güncelleme verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenmiş depo bilgileri</returns>
    /// <response code="200">Depo başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Depo bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPut("{id}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<WarehouseDto>> Update(
        Guid id,
        [FromBody] UpdateWarehouseDto updateDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // Mevcut warehouse'u çek
        var existingQuery = new GetWarehouseByIdQuery(id);
        var existingWarehouse = await _mediator.Send(existingQuery, cancellationToken);
        if (existingWarehouse == null)
        {
            return NotFound();
        }

        var command = new UpdateWarehouseCommand(
            id,
            updateDto.Name ?? existingWarehouse.Name,
            updateDto.Address ?? existingWarehouse.Address,
            updateDto.City ?? existingWarehouse.City,
            updateDto.Country ?? existingWarehouse.Country,
            updateDto.PostalCode ?? existingWarehouse.PostalCode,
            updateDto.ContactPerson ?? existingWarehouse.ContactPerson,
            updateDto.ContactPhone ?? existingWarehouse.ContactPhone,
            updateDto.ContactEmail ?? existingWarehouse.ContactEmail,
            updateDto.Capacity ?? existingWarehouse.Capacity,
            updateDto.IsActive ?? existingWarehouse.IsActive,
            updateDto.Description);
        var warehouse = await _mediator.Send(command, cancellationToken);
        return Ok(warehouse);
    }

    /// <summary>
    /// Depoyu siler (Admin only)
    /// </summary>
    /// <param name="id">Depo ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Depo başarıyla silindi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Depo bulunamadı</response>
    /// <response code="422">İş kuralı ihlali (örn: depoda envanter mevcut)</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpDelete("{id}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteWarehouseCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Depoyu aktifleştirir (Admin only)
    /// </summary>
    /// <param name="id">Depo ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Depo başarıyla aktifleştirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Depo bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{id}/activate")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new ActivateWarehouseCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Depoyu deaktifleştirir (Admin only)
    /// </summary>
    /// <param name="id">Depo ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Depo başarıyla deaktifleştirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu işlem için yetki yok</response>
    /// <response code="404">Depo bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{id}/deactivate")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeactivateWarehouseCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
