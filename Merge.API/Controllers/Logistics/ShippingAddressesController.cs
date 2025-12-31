using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.DTOs.Logistics;


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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShippingAddressDto>>> GetMyAddresses([FromQuery] bool? isActive = null)
    {
        var userId = GetUserId();
        var addresses = await _shippingAddressService.GetUserShippingAddressesAsync(userId, isActive);
        return Ok(addresses);
    }

    [HttpGet("default")]
    public async Task<ActionResult<ShippingAddressDto>> GetDefaultAddress()
    {
        var userId = GetUserId();
        var address = await _shippingAddressService.GetDefaultShippingAddressAsync(userId);
        if (address == null)
        {
            return NotFound();
        }
        return Ok(address);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ShippingAddressDto>> GetAddress(Guid id)
    {
        var userId = GetUserId();
        var address = await _shippingAddressService.GetShippingAddressByIdAsync(id);
        if (address == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only view their own shipping addresses or must be Admin/Manager
        if (address.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(address);
    }

    [HttpPost]
    public async Task<ActionResult<ShippingAddressDto>> CreateAddress([FromBody] CreateShippingAddressDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var address = await _shippingAddressService.CreateShippingAddressAsync(userId, dto);
        return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, address);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] UpdateShippingAddressDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var address = await _shippingAddressService.GetShippingAddressByIdAsync(id);
        if (address == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only update their own shipping addresses or must be Admin/Manager
        if (address.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var success = await _shippingAddressService.UpdateShippingAddressAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAddress(Guid id)
    {
        var userId = GetUserId();
        var address = await _shippingAddressService.GetShippingAddressByIdAsync(id);
        if (address == null)
        {
            return NotFound();
        }

        // ✅ SECURITY: Authorization check - Users can only delete their own shipping addresses or must be Admin/Manager
        if (address.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var success = await _shippingAddressService.DeleteShippingAddressAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/set-default")]
    public async Task<IActionResult> SetDefaultAddress(Guid id)
    {
        var userId = GetUserId();
        var success = await _shippingAddressService.SetDefaultShippingAddressAsync(userId, id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }
}

