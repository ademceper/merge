using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Organization;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Application.Configuration;
using System.Text.Json;
using OrganizationEntity = Merge.Domain.Modules.Identity.Organization;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Application.DTOs.Organization;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using Merge.Domain.SharedKernel.DomainEvents;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.Organization;

public class OrganizationService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrganizationService> logger, IOptions<PaginationSettings> paginationSettings) : IOrganizationService
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Organization oluşturuluyor. Name: {Name}, LegalName: {LegalName}, TaxNumber: {TaxNumber}",
            dto.Name, dto.LegalName, dto.TaxNumber);

        var organization = OrganizationEntity.Create(
            dto.Name,
            dto.LegalName,
            dto.TaxNumber,
            dto.RegistrationNumber,
            dto.Email,
            dto.Phone,
            dto.Website,
            dto.Address,
            dto.AddressLine2, // ✅ AddressLine2 parametresi eklendi
            dto.City,
            dto.State,
            dto.PostalCode,
            dto.Country,
            dto.Settings is not null ? JsonSerializer.Serialize(dto.Settings) : null);

        await context.Set<OrganizationEntity>().AddAsync(organization, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        organization = await context.Set<OrganizationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organization.Id, cancellationToken);

        logger.LogInformation(
            "Organization oluşturuldu. OrganizationId: {OrganizationId}, Name: {Name}",
            organization!.Id, organization.Name);

        return mapper.Map<OrganizationDto>(organization);
    }

    public async Task<OrganizationDto?> GetOrganizationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var organization = await context.Set<OrganizationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (organization is null) return null;

        var userCount = await context.Users
            .AsNoTracking()
            .CountAsync(u => u.OrganizationId == organization.Id, cancellationToken);

        var teamCount = await context.Set<Team>()
            .AsNoTracking()
            .CountAsync(t => t.OrganizationId == organization.Id, cancellationToken);

        var dto = mapper.Map<OrganizationDto>(organization);
        return dto with { UserCount = userCount, TeamCount = teamCount };
    }

    public async Task<PagedResult<OrganizationDto>> GetAllOrganizationsAsync(string? status = null, int page = 1, int pageSize = 0, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0) pageSize = paginationConfig.DefaultPageSize;
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        var query = context.Set<OrganizationEntity>()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            if (Enum.TryParse<EntityStatus>(status, true, out var statusEnum))
            {
                query = query.Where(o => o.Status == statusEnum);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var organizationIds = await query
            .Select(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var organizations = await query
            .OrderBy(o => o.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        var userCounts = await context.Users
            .AsNoTracking()
            .Where(u => organizationIds.Contains(u.OrganizationId ?? Guid.Empty))
            .GroupBy(u => u.OrganizationId)
            .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OrganizationId ?? Guid.Empty, x => x.Count, cancellationToken);

        var teamCounts = await context.Set<Team>()
            .AsNoTracking()
            .Where(t => organizationIds.Contains(t.OrganizationId))
            .GroupBy(t => t.OrganizationId)
            .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OrganizationId, x => x.Count, cancellationToken);

        var result = mapper.Map<IEnumerable<OrganizationDto>>(organizations).ToList();
        
        for (int i = 0; i < result.Count; i++)
        {
            var dto = result[i];
            result[i] = dto with
            {
                UserCount = userCounts.TryGetValue(dto.Id, out var uc) ? uc : 0,
                TeamCount = teamCounts.TryGetValue(dto.Id, out var tc) ? tc : 0
            };
        }

        return new PagedResult<OrganizationDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> UpdateOrganizationAsync(Guid id, UpdateOrganizationDto dto, CancellationToken cancellationToken = default)
    {
        var organization = await context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (organization is null) return false;

        organization.Update(
            dto.Name,
            dto.LegalName,
            dto.TaxNumber,
            dto.RegistrationNumber,
            dto.Email,
            dto.Phone,
            dto.Website,
            dto.Address,
            dto.AddressLine2, // ✅ AddressLine2 parametresi eklendi
            dto.City,
            dto.State,
            dto.PostalCode,
            dto.Country,
            dto.Settings is not null ? JsonSerializer.Serialize(dto.Settings) : null);

        // Status update (separate domain method)
        if (!string.IsNullOrEmpty(dto.Status))
        {
            if (Enum.TryParse<EntityStatus>(dto.Status, true, out var statusEnum))
            {
                if (statusEnum == EntityStatus.Active && organization.Status != EntityStatus.Active)
                    organization.Activate();
                else if (statusEnum == EntityStatus.Suspended && organization.Status != EntityStatus.Suspended)
                    organization.Suspend();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteOrganizationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var organization = await context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (organization is null) return false;

        organization.Delete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> VerifyOrganizationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var organization = await context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (organization is null) return false;

        organization.Verify();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> SuspendOrganizationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var organization = await context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (organization is null) return false;

        organization.Suspend();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<TeamDto> CreateTeamAsync(CreateTeamDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Team oluşturuluyor. OrganizationId: {OrganizationId}, Name: {Name}",
            dto.OrganizationId, dto.Name);

        var organization = await context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == dto.OrganizationId, cancellationToken);

        if (organization is null)
        {
            throw new NotFoundException("Organizasyon", dto.OrganizationId);
        }

        var team = Team.Create(
            dto.OrganizationId,
            dto.Name,
            dto.Description,
            dto.TeamLeadId,
            dto.Settings is not null ? JsonSerializer.Serialize(dto.Settings) : null);

        await context.Set<Team>().AddAsync(team, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        team = await context.Set<Team>()
            .AsNoTracking()
            .Include(t => t.Organization)
            .Include(t => t.TeamLead)
            .FirstOrDefaultAsync(t => t.Id == team.Id, cancellationToken);

        if (team is null)
        {
            logger.LogWarning("Team not found after creation");
            throw new NotFoundException("Team", Guid.Empty);
        }

        var teamDto = mapper.Map<TeamDto>(team);
        
        var memberCount = await context.Set<TeamMember>()
            .AsNoTracking()
            .CountAsync(tm => tm.TeamId == team.Id && tm.IsActive, cancellationToken);
        
        teamDto = teamDto with { MemberCount = memberCount };

        logger.LogInformation(
            "Team oluşturuldu. TeamId: {TeamId}, OrganizationId: {OrganizationId}, Name: {Name}",
            team.Id, dto.OrganizationId, dto.Name);

        return teamDto;
    }

    public async Task<TeamDto?> GetTeamByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var team = await context.Set<Team>()
            .AsNoTracking()
            .Include(t => t.Organization)
            .Include(t => t.TeamLead)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team is null) return null;

        var memberCount = await context.Set<TeamMember>()
            .AsNoTracking()
            .CountAsync(tm => tm.TeamId == team.Id && tm.IsActive, cancellationToken);

        var dto = mapper.Map<TeamDto>(team);
        return dto with { MemberCount = memberCount };
    }

    public async Task<PagedResult<TeamDto>> GetOrganizationTeamsAsync(Guid organizationId, bool? isActive = null, int page = 1, int pageSize = 0, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0) pageSize = paginationConfig.DefaultPageSize;
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        var query = context.Set<Team>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.Organization)
            .Include(t => t.TeamLead)
            .Where(t => t.OrganizationId == organizationId);

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var teamIds = await query
            .Select(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var teams = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        var memberCounts = await context.Set<TeamMember>()
            .AsNoTracking()
            .Where(tm => teamIds.Contains(tm.TeamId) && tm.IsActive)
            .GroupBy(tm => tm.TeamId)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeamId, x => x.Count, cancellationToken);

        var result = mapper.Map<IEnumerable<TeamDto>>(teams).ToList();
        
        for (int i = 0; i < result.Count; i++)
        {
            var dto = result[i];
            result[i] = dto with { MemberCount = memberCounts.TryGetValue(dto.Id, out var count) ? count : 0 };
        }

        return new PagedResult<TeamDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> UpdateTeamAsync(Guid id, UpdateTeamDto dto, CancellationToken cancellationToken = default)
    {
        var team = await context.Set<Team>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team is null) return false;

        team.Update(
            dto.Name,
            dto.Description,
            dto.TeamLeadId,
            dto.Settings is not null ? JsonSerializer.Serialize(dto.Settings) : null);

        // IsActive update (separate domain method)
        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value && !team.IsActive)
                team.Activate();
            else if (!dto.IsActive.Value && team.IsActive)
                team.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteTeamAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var team = await context.Set<Team>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team is null) return false;

        team.Delete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<TeamMemberDto> AddTeamMemberAsync(Guid teamId, AddTeamMemberDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Team member ekleniyor. TeamId: {TeamId}, UserId: {UserId}, Role: {Role}",
            teamId, dto.UserId, dto.Role);

        var team = await context.Set<Team>()
            .FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);

        if (team is null)
        {
            throw new NotFoundException("Takım", teamId);
        }

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Kullanıcı", dto.UserId);
        }

        // Check if user is already a member
        var existing = await context.Set<TeamMember>()
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == dto.UserId, cancellationToken);

        if (existing is not null)
        {
            throw new BusinessException("Kullanıcı zaten bu takımın üyesi.");
        }

        // Parse Role from string to enum
        if (!Enum.TryParse<TeamMemberRole>(dto.Role, true, out var role))
        {
            logger.LogWarning("Invalid TeamMemberRole: {Role}, defaulting to Member", dto.Role);
            role = TeamMemberRole.Member;
        }

        var teamMember = TeamMember.Create(teamId, dto.UserId, role);

        team.AddDomainEvent(new TeamMemberAddedEvent(teamMember.Id, teamId, dto.UserId, role.ToString()));

        await context.Set<TeamMember>().AddAsync(teamMember, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        teamMember = await context.Set<TeamMember>()
            .AsNoTracking()
            .Include(tm => tm.Team)
            .Include(tm => tm.User)
            .FirstOrDefaultAsync(tm => tm.Id == teamMember.Id, cancellationToken);

        logger.LogInformation(
            "Team member eklendi. TeamMemberId: {TeamMemberId}, TeamId: {TeamId}, UserId: {UserId}",
            teamMember!.Id, teamId, dto.UserId);

        return mapper.Map<TeamMemberDto>(teamMember);
    }

    public async Task<bool> RemoveTeamMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
    {
        var teamMember = await context.Set<TeamMember>()
            .Include(tm => tm.Team)
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId, cancellationToken);

        if (teamMember is null) return false;

        if (teamMember.Team is Team team)
        {
            team.AddDomainEvent(new TeamMemberRemovedEvent(teamMember.Id, teamId, userId));
        }

        teamMember.Delete();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateTeamMemberAsync(Guid teamId, Guid userId, UpdateTeamMemberDto dto, CancellationToken cancellationToken = default)
    {
        var teamMember = await context.Set<TeamMember>()
            .Include(tm => tm.Team)
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId, cancellationToken);

        if (teamMember is null) return false;

        if (!string.IsNullOrEmpty(dto.Role))
        {
            if (Enum.TryParse<TeamMemberRole>(dto.Role, true, out var role))
            {
                teamMember.UpdateRole(role);
            }
            else
            {
                logger.LogWarning("Invalid TeamMemberRole: {Role}", dto.Role);
            }
        }

        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value && !teamMember.IsActive)
                teamMember.Activate();
            else if (!dto.IsActive.Value && teamMember.IsActive)
                teamMember.Deactivate();
        }

        if (teamMember.Team is Team team)
        {
            team.AddDomainEvent(new TeamMemberUpdatedEvent(teamMember.Id, teamId, userId, teamMember.Role.ToString()));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<PagedResult<TeamMemberDto>> GetTeamMembersAsync(Guid teamId, bool? isActive = null, int page = 1, int pageSize = 0, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0) pageSize = paginationConfig.DefaultPageSize;
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        var query = context.Set<TeamMember>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(tm => tm.Team)
            .Include(tm => tm.User)
            .Where(tm => tm.TeamId == teamId);

        if (isActive.HasValue)
        {
            query = query.Where(tm => tm.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var members = await query
            .OrderBy(tm => tm.Role)
            .ThenBy(tm => tm.User.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var dtos = mapper.Map<IEnumerable<TeamMemberDto>>(members);

        return new PagedResult<TeamMemberDto>
        {
            Items = dtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<TeamDto>> GetUserTeamsAsync(Guid userId, int page = 1, int pageSize = 0, CancellationToken cancellationToken = default)
    {
        if (pageSize <= 0) pageSize = paginationConfig.DefaultPageSize;
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        var teamIdsQuery = context.Set<TeamMember>()
            .AsNoTracking()
            .Where(tm => tm.UserId == userId && tm.IsActive)
            .Select(tm => tm.TeamId)
            .Distinct();

        var totalCount = await teamIdsQuery.CountAsync(cancellationToken);

        var teamIds = await teamIdsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var teamMembers = await context.Set<TeamMember>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(tm => tm.Team)
                .ThenInclude(t => t.Organization)
            .Include(tm => tm.Team)
                .ThenInclude(t => t.TeamLead)
            .Where(tm => tm.UserId == userId && tm.IsActive && teamIds.Contains(tm.TeamId))
            .Select(tm => tm.Team)
            .Distinct()
            .ToListAsync(cancellationToken);
        
        var memberCounts = await context.Set<TeamMember>()
            .AsNoTracking()
            .Where(tm => teamIds.Contains(tm.TeamId) && tm.IsActive)
            .GroupBy(tm => tm.TeamId)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeamId, x => x.Count, cancellationToken);

        var result = mapper.Map<IEnumerable<TeamDto>>(teamMembers).ToList();
        
        for (int i = 0; i < result.Count; i++)
        {
            var dto = result[i];
            result[i] = dto with { MemberCount = memberCounts.TryGetValue(dto.Id, out var count) ? count : 0 };
        }

        return new PagedResult<TeamDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

}

