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
public class ShippingAddressesController(IMediator mediator) : BaseController
{
    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ShippingAddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ShippingAddressDto>>> GetMyAddresses(
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetUserShippingAddressesQuery(userId, isActive);
        var addresses = await mediator.Send(query, cancellationToken);
        return Ok(addresses);
    }

    [HttpGet("default")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ShippingAddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ShippingAddressDto>> GetDefaultAddress(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetDefaultShippingAddressQuery(userId);
        var address = await mediator.Send(query, cancellationToken);
        if (address == null)
        {
            return NotFound();
        }
        return Ok(address);
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
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
        var address = await mediator.Send(query, cancellationToken);
        if (address == null)
        {
            return NotFound();
        }

        if (address.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        return Ok(address);
    }

    [HttpPost]
    [RateLimit(10, 60)]
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
        var address = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, address);
    }

    [HttpPut("{id}")]
    [RateLimit(20, 60)]
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
        var address = await mediator.Send(addressQuery, cancellationToken);
        if (address == null)
        {
            return NotFound();
        }

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
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Kargo adresini kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PatchAddress(
        Guid id,
        [FromBody] PatchShippingAddressDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var userId = GetUserId();
        var addressQuery = new GetShippingAddressByIdQuery(id);
        var address = await mediator.Send(addressQuery, cancellationToken);
        if (address == null)
        {
            return NotFound();
        }
        if (address.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new UpdateShippingAddressCommand(
            id,
            patchDto.Label,
            patchDto.FirstName,
            patchDto.LastName,
            patchDto.Phone,
            patchDto.AddressLine1,
            patchDto.City,
            patchDto.State,
            patchDto.PostalCode,
            patchDto.Country,
            patchDto.AddressLine2,
            patchDto.IsDefault,
            patchDto.IsActive,
            patchDto.Instructions);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [RateLimit(10, 60)]
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
        var address = await mediator.Send(addressQuery, cancellationToken);
        if (address == null)
        {
            return NotFound();
        }

        if (address.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        var command = new DeleteShippingAddressCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/set-default")]
    [RateLimit(10, 60)]
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
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
