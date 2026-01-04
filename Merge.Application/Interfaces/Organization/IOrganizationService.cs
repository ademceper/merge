using Merge.Application.DTOs.Organization;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Organization;

public interface IOrganizationService
{
    Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto, CancellationToken cancellationToken = default);
    Task<OrganizationDto?> GetOrganizationByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<OrganizationDto>> GetAllOrganizationsAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> UpdateOrganizationAsync(Guid id, UpdateOrganizationDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteOrganizationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> VerifyOrganizationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SuspendOrganizationAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TeamDto> CreateTeamAsync(CreateTeamDto dto, CancellationToken cancellationToken = default);
    Task<TeamDto?> GetTeamByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<TeamDto>> GetOrganizationTeamsAsync(Guid organizationId, bool? isActive = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> UpdateTeamAsync(Guid id, UpdateTeamDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteTeamAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TeamMemberDto> AddTeamMemberAsync(Guid teamId, AddTeamMemberDto dto, CancellationToken cancellationToken = default);
    Task<bool> RemoveTeamMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateTeamMemberAsync(Guid teamId, Guid userId, UpdateTeamMemberDto dto, CancellationToken cancellationToken = default);
    Task<PagedResult<TeamMemberDto>> GetTeamMembersAsync(Guid teamId, bool? isActive = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PagedResult<TeamDto>> GetUserTeamsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}

