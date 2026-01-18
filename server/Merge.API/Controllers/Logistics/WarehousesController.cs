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
using Merge.Application.Logistics.Commands.PatchWarehouse;
using Merge.Application.Logistics.Commands.DeleteWarehouse;
using Merge.Application.Logistics.Commands.ActivateWarehouse;
using Merge.Application.Logistics.Commands.DeactivateWarehouse;

namespace Merge.API.Controllers.Logistics;

/// <summary>
/// Warehouses API endpoints.
/// Depo işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/logistics/warehouses")]
[Authorize(Roles = "Admin")]
[Tags("Warehouses")]
public class WarehousesController(IMediator mediator) : BaseController
{
    [HttpGet]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetAll(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllWarehousesQuery(includeInactive);
        var warehouses = await mediator.Send(query, cancellationToken);
        return Ok(warehouses);
    }

    [HttpGet("active")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<WarehouseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetActive(
        CancellationToken cancellationToken = default)
    {
        var query = new GetActiveWarehousesQuery();
        var warehouses = await mediator.Send(query, cancellationToken);
        return Ok(warehouses);
    }

    [HttpGet("{id}")]
    [RateLimit(60, 60)]
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
        var warehouse = await mediator.Send(query, cancellationToken);
        if (warehouse == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(warehouse);
    }

    [HttpGet("code/{code}")]
    [RateLimit(60, 60)]
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
        var warehouse = await mediator.Send(query, cancellationToken);
        if (warehouse == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }
        return Ok(warehouse);
    }

    [HttpPost]
    [RateLimit(10, 60)]
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
        var warehouse = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
    }

    [HttpPut("{id}")]
    [RateLimit(20, 60)]
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
        var existingWarehouse = await mediator.Send(existingQuery, cancellationToken);
        if (existingWarehouse == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
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
        var warehouse = await mediator.Send(command, cancellationToken);
        return Ok(warehouse);
    }

    /// <summary>
    /// Depoyu kısmi olarak günceller (PATCH)
    /// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
    /// </summary>
    [HttpPatch("{id}")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<WarehouseDto>> Patch(
        Guid id,
        [FromBody] PatchWarehouseDto patchDto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var existingQuery = new GetWarehouseByIdQuery(id);
        var existingWarehouse = await mediator.Send(existingQuery, cancellationToken);
        if (existingWarehouse == null)
        {
            return Problem("Resource not found", "Not Found", StatusCodes.Status404NotFound);
        }

        var command = new PatchWarehouseCommand(id, patchDto);
        var warehouse = await mediator.Send(command, cancellationToken);
        return Ok(warehouse);
    }

    [HttpDelete("{id}")]
    [RateLimit(10, 60)]
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
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/activate")]
    [RateLimit(10, 60)]
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
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/deactivate")]
    [RateLimit(10, 60)]
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
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
