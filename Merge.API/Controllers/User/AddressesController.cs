using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.DTOs.User;
using Merge.Infrastructure.Data;


namespace Merge.API.Controllers.User;

[ApiController]
[Route("api/user/addresses")]
[Authorize]
public class AddressesController : BaseController
{
    private readonly IAddressService _addressService;
    private readonly ApplicationDbContext _context;

    public AddressesController(IAddressService addressService, ApplicationDbContext context)
    {
        _addressService = addressService;
        _context = context;
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
        var userId = GetUserId();
        
        // ✅ AUTHORIZATION: Kullanıcı sadece kendi adreslerine erişebilmeli
        var addressEntity = await _context.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        
        if (addressEntity == null)
        {
            return NotFound();
        }
        
        if (addressEntity.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        var address = await _addressService.GetByIdAsync(id);
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

        var userId = GetUserId();
        
        // ✅ AUTHORIZATION: Kullanıcı sadece kendi adreslerini güncelleyebilmeli
        var addressEntity = await _context.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        
        if (addressEntity == null)
        {
            return NotFound();
        }
        
        if (addressEntity.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var updatedAddress = await _addressService.UpdateAsync(id, dto);
        return Ok(updatedAddress);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        
        // ✅ AUTHORIZATION: Kullanıcı sadece kendi adreslerini silebilmeli
        var addressEntity = await _context.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        
        if (addressEntity == null)
        {
            return NotFound();
        }
        
        if (addressEntity.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
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

