using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Identity;
using Merge.Application.Identity.Queries.GetAllPermissions;

namespace Merge.API.Controllers.Identity;

/// <summary>
/// Permissions API endpoints.
/// İzin yönetimi işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/permissions")]
[Tags("Permissions")]
[Authorize]
public class PermissionsController(IMediator mediator) : BaseController
{
    /// <summary>
    /// Tüm izinleri getirir
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "RequireAdminRole")]
    [ProducesResponseType(typeof(List<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PermissionDto>>> GetAll(
        [FromQuery] string? category = null,
        [FromQuery] string? resource = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllPermissionsQuery(category, resource);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
