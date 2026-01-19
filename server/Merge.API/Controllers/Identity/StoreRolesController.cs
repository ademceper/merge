using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Identity;
using Merge.Application.Identity.Queries.GetStoreRoles;
using Merge.Application.Identity.Commands.AssignStoreRole;
using Merge.Application.Identity.Commands.RemoveStoreRole;

namespace Merge.API.Controllers.Identity;

/// <summary>
/// Store Roles API endpoints.
/// Store bazlı rol atamalarını yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/store-roles")]
[Tags("Store Roles")]
[Authorize]
public class StoreRolesController(IMediator mediator) : BaseController
{
    /// <summary>
    /// Store rollerini getirir
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(List<StoreRoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StoreRoleDto>>> GetAll(
        [FromQuery] Guid? storeId = null,
        [FromQuery] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStoreRolesQuery(storeId, userId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Store rolü atar
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(StoreRoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StoreRoleDto>> Assign(
        [FromBody] AssignStoreRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var assignedBy = GetUserId();
        command = command with { AssignedByUserId = assignedBy };
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    /// <summary>
    /// Store rolünü kaldırır
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveStoreRoleCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
