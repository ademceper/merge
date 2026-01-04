using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.DTOs.Logistics;
using Merge.API.Middleware;

namespace Merge.API.Controllers.Logistics;

[ApiController]
[Route("api/logistics/shipping-addresses")]
[Authorize]
public class ShippingAddressesController : BaseController
{
    private readonly IShippingAddressService _shippingAddressService;

    public ShippingAddressesController(IShippingAddressService shippingAddressService)
    {
        _shippingAddressService = shippingAddressService;
    }

    /// <summary>
    /// Kullanıcının kargo adreslerini getirir
    /// </summary>
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var addresses = await _shippingAddressService.GetUserShippingAddressesAsync(userId, isActive, cancellationToken);
        return Ok(addresses);
    }

    /// <summary>
    /// Kullanıcının varsayılan kargo adresini getirir
    /// </summary>
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var address = await _shippingAddressService.GetDefaultShippingAddressAsync(userId, cancellationToken);
        if (address == null)
        {
            return NotFound();
        }
        return Ok(address);
    }

    /// <summary>
    /// Kargo adresi detaylarını getirir
    /// </summary>
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var address = await _shippingAddressService.GetShippingAddressByIdAsync(id, cancellationToken);
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var address = await _shippingAddressService.CreateShippingAddressAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, address);
    }

    /// <summary>
    /// Kargo adresini günceller
    /// </summary>
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var address = await _shippingAddressService.GetShippingAddressByIdAsync(id, cancellationToken);
        if (address == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi adreslerini güncelleyebilir
        if (address.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _shippingAddressService.UpdateShippingAddressAsync(id, dto, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Kargo adresini siler
    /// </summary>
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var address = await _shippingAddressService.GetShippingAddressByIdAsync(id, cancellationToken);
        if (address == null)
        {
            return NotFound();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi adreslerini silebilir
        if (address.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _shippingAddressService.DeleteShippingAddressAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Kargo adresini varsayılan olarak ayarlar
    /// </summary>
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
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var success = await _shippingAddressService.SetDefaultShippingAddressAsync(userId, id, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}

