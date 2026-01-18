using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.DTOs.User;
using Merge.Application.User.Commands.CreateAddress;
using Merge.Application.User.Commands.DeleteAddress;
using Merge.Application.User.Commands.SetDefaultAddress;
using Merge.Application.User.Commands.UpdateAddress;
using Merge.Application.User.Commands.PatchAddress;
using Merge.Application.User.Queries.GetAddressById;
using Merge.Application.User.Queries.GetAddressesByUserId;
using Merge.API.Middleware;

namespace Merge.API.Controllers.User;

/// <summary>
/// User Addresses API endpoints.
/// Kullanıcı adreslerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/user/addresses")]
[Authorize]
[Tags("UserAddresses")]
public class AddressesController(IMediator mediator) : BaseController
{

    [HttpGet]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(IEnumerable<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetMyAddresses(CancellationToken cancellationToken = default)
    {
                        var userId = GetUserId();
        var query = new GetAddressesByUserIdQuery(userId);
        var addresses = await mediator.Send(query, cancellationToken);
        return Ok(addresses);
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]     [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AddressDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
                        var userId = GetUserId();
        var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
        
                var query = new GetAddressByIdQuery(id, userId, isAdminOrManager);
        var address = await mediator.Send(query, cancellationToken);
        if (address == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(address);
    }

    [HttpPost]
    [RateLimit(30, 60)]     [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AddressDto>> Create([FromBody] CreateAddressDto dto, CancellationToken cancellationToken = default)
    {
                var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
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
        var address = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = address.Id }, address);
    }

    [HttpPut("{id}")]
    [RateLimit(30, 60)]     [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AddressDto>> Update(Guid id, [FromBody] UpdateAddressDto dto, CancellationToken cancellationToken = default)
    {
                var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");

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
        var updatedAddress = await mediator.Send(command, cancellationToken);
        return Ok(updatedAddress);
    }

    /// <summary>
    /// Adresi kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [RateLimit(30, 60)]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AddressDto>> Patch(Guid id, [FromBody] PatchAddressDto patchDto, CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");

        var command = new PatchAddressCommand(id, patchDto, userId, isAdminOrManager);
        var updatedAddress = await mediator.Send(command, cancellationToken);
        return Ok(updatedAddress);
    }

    [HttpDelete("{id}")]
    [RateLimit(30, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
                var userId = GetUserId();
        var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");
        
                        var command = new DeleteAddressCommand(id, userId, isAdminOrManager);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

    [HttpPost("{id}/set-default")]
    [RateLimit(30, 60)]     [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken = default)
    {
                var userId = GetUserId();
        
                        var command = new SetDefaultAddressCommand(id, userId);
        var result = await mediator.Send(command, cancellationToken);
        if (!result)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return NoContent();
    }

}
