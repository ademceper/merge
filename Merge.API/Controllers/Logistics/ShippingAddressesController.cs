using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.API.Middleware;
using Merge.Application.Logistics.Queries.GetUserShippingAddresses;
using Merge.Application.Logistics.Queries.GetDefaultShippingAddress;
using Merge.Application.Logistics.Queries.GetShippingAddressById;
using Merge.Application.Logistics.Commands.CreateShippingAddress;
using Merge.Application.Logistics.Commands.UpdateShippingAddress;
using Merge.Application.Logistics.Commands.DeleteShippingAddress;
using Merge.Application.Logistics.Commands.SetDefaultShippingAddress;

namespace Merge.API.Controllers.Logistics;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/logistics/shipping-addresses")]
[Authorize]
public class ShippingAddressesController : BaseController
{
    private readonly IMediator _mediator;

    public ShippingAddressesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Kullanıcının kargo adreslerini getirir
    /// </summary>
    /// <param name="isActive">Sadece aktif adresleri getir (opsiyonel)</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kullanıcının kargo adresleri listesi</returns>
    /// <response code="200">Kargo adresleri başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ShippingAddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ShippingAddressDto>>> GetMyAddresses(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetUserShippingAddressesQuery(userId, isActive);
        var addresses = await _mediator.Send(query, cancellationToken);
        return Ok(addresses);
    }

    /// <summary>
    /// Kullanıcının varsayılan kargo adresini getirir
    /// </summary>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Varsayılan kargo adresi</returns>
    /// <response code="200">Varsayılan kargo adresi başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="404">Varsayılan kargo adresi bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("default")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ShippingAddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingAddressDto>> GetDefaultAddress(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetDefaultShippingAddressQuery(userId);
        var address = await _mediator.Send(query, cancellationToken);
        if (address == null)
        {
            return NotFound();
        }
        return Ok(address);
    }

    /// <summary>
    /// Kargo adresi detaylarını getirir
    /// </summary>
    /// <param name="id">Kargo adresi ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Kargo adresi detayları</returns>
    /// <response code="200">Kargo adresi başarıyla getirildi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu adrese erişim yetkisi yok</response>
    /// <response code="404">Kargo adresi bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ShippingAddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingAddressDto>> GetAddress(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetShippingAddressByIdQuery(id);
        var address = await _mediator.Send(query, cancellationToken);
        if (address == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi adreslerine erişebilir
        if (address.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(address);
    }

    /// <summary>
    /// Yeni kargo adresi oluşturur
    /// </summary>
    /// <param name="dto">Kargo adresi oluşturma verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Oluşturulan kargo adresi bilgileri</returns>
    /// <response code="201">Kargo adresi başarıyla oluşturuldu</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(ShippingAddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingAddressDto>> CreateAddress(
        [FromBody] CreateShippingAddressDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var command = new CreateShippingAddressCommand(
            userId,
            dto.Label,
            dto.FirstName,
            dto.LastName,
            dto.Phone,
            dto.AddressLine1,
            dto.City,
            dto.State ?? string.Empty,
            dto.PostalCode ?? string.Empty,
            dto.Country,
            dto.AddressLine2,
            dto.IsDefault,
            dto.Instructions);
        var address = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, address);
    }

    /// <summary>
    /// Kargo adresini günceller
    /// </summary>
    /// <param name="id">Kargo adresi ID'si</param>
    /// <param name="dto">Güncelleme verileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Kargo adresi başarıyla güncellendi</response>
    /// <response code="400">Geçersiz istek verisi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu adresi güncelleme yetkisi yok</response>
    /// <response code="404">Kargo adresi bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPut("{id}")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateAddress(
        Guid id,
        [FromBody] UpdateShippingAddressDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var addressQuery = new GetShippingAddressByIdQuery(id);
        var address = await _mediator.Send(addressQuery, cancellationToken);
        if (address == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi adreslerini güncelleyebilir
        if (address.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new UpdateShippingAddressCommand(
            id,
            dto.Label,
            dto.FirstName,
            dto.LastName,
            dto.Phone,
            dto.AddressLine1,
            dto.City,
            dto.State,
            dto.PostalCode,
            dto.Country,
            dto.AddressLine2,
            dto.IsDefault,
            dto.IsActive,
            dto.Instructions);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Kargo adresini siler
    /// </summary>
    /// <param name="id">Kargo adresi ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Kargo adresi başarıyla silindi</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="403">Bu adresi silme yetkisi yok</response>
    /// <response code="404">Kargo adresi bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpDelete("{id}")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteAddress(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var addressQuery = new GetShippingAddressByIdQuery(id);
        var address = await _mediator.Send(addressQuery, cancellationToken);
        if (address == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi adreslerini silebilir
        if (address.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new DeleteShippingAddressCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Kargo adresini varsayılan olarak ayarlar
    /// </summary>
    /// <param name="id">Kargo adresi ID'si</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı (204 No Content)</returns>
    /// <response code="204">Kargo adresi varsayılan olarak ayarlandı</response>
    /// <response code="401">Kullanıcı kimlik doğrulaması gerekli</response>
    /// <response code="404">Kargo adresi bulunamadı</response>
    /// <response code="429">Çok fazla istek</response>
    [HttpPost("{id}/set-default")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SetDefaultAddress(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var command = new SetDefaultShippingAddressCommand(userId, id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

