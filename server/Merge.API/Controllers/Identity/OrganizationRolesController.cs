using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Identity;
using Merge.Application.Identity.Queries.GetOrganizationRoles;
using Merge.Application.Identity.Commands.AssignOrganizationRole;
using Merge.Application.Identity.Commands.RemoveOrganizationRole;

namespace Merge.API.Controllers.Identity;

/// <summary>
/// Organization Roles API endpoints.
/// Organizasyon bazlı rol atamalarını yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/organization-roles")]
[Tags("Organization Roles")]
[Authorize]
public class OrganizationRolesController(IMediator mediator) : BaseController
{
    /// <summary>
    /// Organizasyon rollerini getirir
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(List<OrganizationRoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrganizationRoleDto>>> GetAll(
        [FromQuery] Guid? organizationId = null,
        [FromQuery] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrganizationRolesQuery(organizationId, userId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Organizasyon rolü atar
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(OrganizationRoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrganizationRoleDto>> Assign(
        [FromBody] AssignOrganizationRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var assignedBy = GetUserId();
        command = command with { AssignedByUserId = assignedBy };
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    /// <summary>
    /// Organizasyon rolünü kaldırır
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveOrganizationRoleCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
