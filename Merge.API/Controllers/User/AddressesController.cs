using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.User;
using Merge.Application.User.Commands.CreateAddress;
using Merge.Application.User.Commands.DeleteAddress;
using Merge.Application.User.Commands.SetDefaultAddress;
using Merge.Application.User.Commands.UpdateAddress;
using Merge.Application.User.Queries.GetAddressById;
using Merge.Application.User.Queries.GetAddressesByUserId;
using Merge.API.Middleware;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.2: IDOR koruması (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
namespace Merge.API.Controllers.User;

[ApiController]
[Route("api/v{version:apiVersion}/user/addresses")]
[Authorize]
public class AddressesController : BaseController
{
    private readonly IMediator _mediator;

    public AddressesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU) - Kullanıcı sadece kendi adreslerine erişebilir
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(IEnumerable<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetMyAddresses(CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var userId = GetUserId();
        var query = new GetAddressesByUserIdQuery(userId);
        var addresses = await _mediator.Send(query, cancellationToken);
        return Ok(addresses);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU) - Kullanıcı sadece kendi adreslerine erişebilir
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("{id}")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AddressDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR koruması - Handler'da kontrol ediliyor
        var userId = GetUserId();
        var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new GetAddressByIdQuery(id, userId, isAdminOrManager);
        var address = await _mediator.Send(query, cancellationToken);
        if (address == null)
        {
            return NotFound();
        }
        return Ok(address);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AddressDto>> Create([FromBody] CreateAddressDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var command = new CreateAddressCommand(
            userId,
            dto.Title,
            dto.FirstName,
            dto.LastName,
            dto.PhoneNumber,
            dto.AddressLine1,
            dto.AddressLine2,
            dto.City,
            dto.District,
            dto.PostalCode,
            dto.Country,
            dto.IsDefault);
        var address = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = address.Id }, address);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU) - Kullanıcı sadece kendi adreslerini güncelleyebilir
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPut("{id}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AddressDto>> Update(Guid id, [FromBody] UpdateAddressDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR koruması - Handler'da kontrol ediliyor
        var command = new UpdateAddressCommand(
            id,
            userId,
            isAdminOrManager,
            dto.Title,
            dto.FirstName,
            dto.LastName,
            dto.PhoneNumber,
            dto.AddressLine1,
            dto.AddressLine2,
            dto.City,
            dto.District,
            dto.PostalCode,
            dto.Country,
            dto.IsDefault);
        var updatedAddress = await _mediator.Send(command, cancellationToken);
        return Ok(updatedAddress);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU) - Kullanıcı sadece kendi adreslerini silebilir
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpDelete("{id}")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var userId = GetUserId();
        var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR koruması - Handler'da kontrol ediliyor
        var command = new DeleteAddressCommand(id, userId, isAdminOrManager);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.2: IDOR koruması (ZORUNLU) - Kullanıcı sadece kendi adreslerini varsayılan yapabilir
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("{id}/set-default")]
    [RateLimit(30, 60)] // ✅ BOLUM 3.3: Rate Limiting - 30/dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var userId = GetUserId();
        
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR koruması - Handler'da kontrol ediliyor (SetDefaultAddressCommand zaten userId alıyor)
        var command = new SetDefaultAddressCommand(id, userId);
        var result = await _mediator.Send(command, cancellationToken);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }

}

