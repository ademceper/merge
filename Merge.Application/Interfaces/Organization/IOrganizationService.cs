using Merge.Application.DTOs.Organization;

namespace Merge.Application.Interfaces.Organization;

public interface IOrganizationService
{
    Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto);
    Task<OrganizationDto?> GetOrganizationByIdAsync(Guid id);
    Task<IEnumerable<OrganizationDto>> GetAllOrganizationsAsync(string? status = null);
    Task<bool> UpdateOrganizationAsync(Guid id, UpdateOrganizationDto dto);
    Task<bool> DeleteOrganizationAsync(Guid id);
    Task<bool> VerifyOrganizationAsync(Guid id);
    Task<bool> SuspendOrganizationAsync(Guid id);
    Task<TeamDto> CreateTeamAsync(CreateTeamDto dto);
    Task<TeamDto?> GetTeamByIdAsync(Guid id);
    Task<IEnumerable<TeamDto>> GetOrganizationTeamsAsync(Guid organizationId, bool? isActive = null);
    Task<bool> UpdateTeamAsync(Guid id, UpdateTeamDto dto);
    Task<bool> DeleteTeamAsync(Guid id);
    Task<TeamMemberDto> AddTeamMemberAsync(Guid teamId, AddTeamMemberDto dto);
    Task<bool> RemoveTeamMemberAsync(Guid teamId, Guid userId);
    Task<bool> UpdateTeamMemberAsync(Guid teamId, Guid userId, UpdateTeamMemberDto dto);
    Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(Guid teamId, bool? isActive = null);
    Task<IEnumerable<TeamDto>> GetUserTeamsAsync(Guid userId);
}

