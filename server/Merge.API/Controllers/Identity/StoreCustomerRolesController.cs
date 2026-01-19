using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Identity;
using Merge.Application.Identity.Queries.GetStoreCustomerRoles;
using Merge.Application.Identity.Commands.AssignStoreCustomerRole;
using Merge.Application.Identity.Commands.RemoveStoreCustomerRole;

namespace Merge.API.Controllers.Identity;

/// <summary>
/// Store Customer Roles API endpoints.
/// Satıcı web'inde müşteri rol atamalarını yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/store-customer-roles")]
[Tags("Store Customer Roles")]
[Authorize]
public class StoreCustomerRolesController(IMediator mediator) : BaseController
{
    /// <summary>
    /// Store müşteri rollerini getirir
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(List<StoreCustomerRoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StoreCustomerRoleDto>>> GetAll(
        [FromQuery] Guid? storeId = null,
        [FromQuery] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStoreCustomerRolesQuery(storeId, userId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Store müşteri rolü atar
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(StoreCustomerRoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StoreCustomerRoleDto>> Assign(
        [FromBody] AssignStoreCustomerRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var assignedBy = GetUserId();
        command = command with { AssignedByUserId = assignedBy };
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    /// <summary>
    /// Store müşteri rolünü kaldırır
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveStoreCustomerRoleCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
