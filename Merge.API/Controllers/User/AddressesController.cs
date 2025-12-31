using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.DTOs.User;


namespace Merge.API.Controllers.User;

[ApiController]
[Route("api/user/addresses")]
[Authorize]
public class AddressesController : BaseController
{
    private readonly IAddressService _addressService;

    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetMyAddresses()
    {
        var userId = GetUserId();
        var addresses = await _addressService.GetByUserIdAsync(userId);
        return Ok(addresses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AddressDto>> GetById(Guid id)
    {
        var address = await _addressService.GetByIdAsync(id);
        if (address == null)
        {
            return NotFound();
        }
        return Ok(address);
    }

    [HttpPost]
    public async Task<ActionResult<AddressDto>> Create([FromBody] CreateAddressDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        dto.UserId = userId;
        var address = await _addressService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = address.Id }, address);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AddressDto>> Update(Guid id, [FromBody] UpdateAddressDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var address = await _addressService.UpdateAsync(id, dto);
        if (address == null)
        {
            return NotFound();
        }
        return Ok(address);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _addressService.DeleteAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/set-default")]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        var userId = GetUserId();
        var result = await _addressService.SetDefaultAsync(id, userId);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

}

