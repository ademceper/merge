using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.Organization;
using Merge.Application.DTOs.Organization;


namespace Merge.API.Controllers.Organization;

[ApiController]
[Route("api/organizations")]
[Authorize(Roles = "Admin,Manager")]
public class OrganizationsController : BaseController
{
    private readonly IOrganizationService _organizationService;

    public OrganizationsController(IOrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetAllOrganizations([FromQuery] string? status = null)
    {
        var organizations = await _organizationService.GetAllOrganizationsAsync(status);
        return Ok(organizations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrganizationDto>> GetOrganization(Guid id)
    {
        var organization = await _organizationService.GetOrganizationByIdAsync(id);
        if (organization == null)
        {
            return NotFound();
        }
        return Ok(organization);
    }

    [HttpPost]
    public async Task<ActionResult<OrganizationDto>> CreateOrganization([FromBody] CreateOrganizationDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var organization = await _organizationService.CreateOrganizationAsync(dto);
        return CreatedAtAction(nameof(GetOrganization), new { id = organization.Id }, organization);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrganization(Guid id, [FromBody] UpdateOrganizationDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _organizationService.UpdateOrganizationAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrganization(Guid id)
    {
        var success = await _organizationService.DeleteOrganizationAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/verify")]
    public async Task<IActionResult> VerifyOrganization(Guid id)
    {
        var success = await _organizationService.VerifyOrganizationAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPost("{id}/suspend")]
    public async Task<IActionResult> SuspendOrganization(Guid id)
    {
        var success = await _organizationService.SuspendOrganizationAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Teams
    [HttpGet("{organizationId}/teams")]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetOrganizationTeams(Guid organizationId, [FromQuery] bool? isActive = null)
    {
        var teams = await _organizationService.GetOrganizationTeamsAsync(organizationId, isActive);
        return Ok(teams);
    }

    [HttpPost("teams")]
    public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var team = await _organizationService.CreateTeamAsync(dto);
        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team);
    }

    [HttpGet("teams/{id}")]
    public async Task<ActionResult<TeamDto>> GetTeam(Guid id)
    {
        var team = await _organizationService.GetTeamByIdAsync(id);
        if (team == null)
        {
            return NotFound();
        }
        return Ok(team);
    }

    [HttpPut("teams/{id}")]
    public async Task<IActionResult> UpdateTeam(Guid id, [FromBody] UpdateTeamDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _organizationService.UpdateTeamAsync(id, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpDelete("teams/{id}")]
    public async Task<IActionResult> DeleteTeam(Guid id)
    {
        var success = await _organizationService.DeleteTeamAsync(id);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    // Team Members
    [HttpGet("teams/{teamId}/members")]
    public async Task<ActionResult<IEnumerable<TeamMemberDto>>> GetTeamMembers(Guid teamId, [FromQuery] bool? isActive = null)
    {
        var members = await _organizationService.GetTeamMembersAsync(teamId, isActive);
        return Ok(members);
    }

    [HttpPost("teams/{teamId}/members")]
    public async Task<ActionResult<TeamMemberDto>> AddTeamMember(Guid teamId, [FromBody] AddTeamMemberDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var member = await _organizationService.AddTeamMemberAsync(teamId, dto);
        return CreatedAtAction(nameof(GetTeamMembers), new { teamId = teamId }, member);
    }

    [HttpDelete("teams/{teamId}/members/{userId}")]
    public async Task<IActionResult> RemoveTeamMember(Guid teamId, Guid userId)
    {
        var success = await _organizationService.RemoveTeamMemberAsync(teamId, userId);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPut("teams/{teamId}/members/{userId}")]
    public async Task<IActionResult> UpdateTeamMember(Guid teamId, Guid userId, [FromBody] UpdateTeamMemberDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var success = await _organizationService.UpdateTeamMemberAsync(teamId, userId, dto);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpGet("users/{userId}/teams")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetUserTeams(Guid userId)
    {
        var teams = await _organizationService.GetUserTeamsAsync(userId);
        return Ok(teams);
    }
}

