using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Identity;
using Merge.Application.Identity.Queries.GetUserRolesAndPermissions;
using Merge.Application.Identity.Queries.GetAllRoles;
using Merge.Application.Identity.Commands.CreateRole;
using Merge.Domain.Enums;

namespace Merge.API.Controllers.Identity;

/// <summary>
/// Roles API endpoints.
/// Rol ve izin yönetimi işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/roles")]
[Tags("Roles")]
[Authorize]
public class RolesController(IMediator mediator) : BaseController
{
    /// <summary>
    /// Kullanıcının tüm rollerini ve izinlerini getirir
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserRolesAndPermissionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserRolesAndPermissionsDto>> GetMyRolesAndPermissions(
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var query = new GetUserRolesAndPermissionsQuery(userId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Tüm rolleri getirir
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RoleDto>>> GetAll(
        [FromQuery] RoleType? roleType = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllRolesQuery(roleType);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Yeni rol oluşturur
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoleDto>> Create(
        [FromBody] CreateRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }
}
