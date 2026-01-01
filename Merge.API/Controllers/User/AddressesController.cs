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
    [ProducesResponseType(typeof(IEnumerable<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetMyAddresses(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var addresses = await _addressService.GetByUserIdAsync(userId, cancellationToken);
        return Ok(addresses);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AddressDto>> GetById(Guid id, CancellationToken cancellationToken = default)
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
        
        var address = await _addressService.GetByIdAsync(id, cancellationToken);
        return Ok(address);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AddressDto>> Create([FromBody] CreateAddressDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        dto.UserId = userId;
        var address = await _addressService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = address.Id }, address);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AddressDto>> Update(Guid id, [FromBody] UpdateAddressDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        
        // ✅ AUTHORIZATION: Kullanıcı sadece kendi adreslerini güncelleyebilmeli
        var addressEntity = await _context.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        
        if (addressEntity == null)
        {
            return NotFound();
        }
        
        if (addressEntity.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var updatedAddress = await _addressService.UpdateAsync(id, dto, cancellationToken);
        return Ok(updatedAddress);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        
        // ✅ AUTHORIZATION: Kullanıcı sadece kendi adreslerini silebilmeli
        var addressEntity = await _context.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        
        if (addressEntity == null)
        {
            return NotFound();
        }
        
        if (addressEntity.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }
        
        var result = await _addressService.DeleteAsync(id, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/set-default")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _addressService.SetDefaultAsync(id, userId, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

}

