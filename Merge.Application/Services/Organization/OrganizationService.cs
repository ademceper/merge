using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Organization;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using OrganizationEntity = Merge.Domain.Entities.Organization;
using UserEntity = Merge.Domain.Entities.User;
using Merge.Application.DTOs.Organization;
using Merge.Application.Common;


namespace Merge.Application.Services.Organization;

public class OrganizationService : IOrganizationService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrganizationService> _logger;

    public OrganizationService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<OrganizationService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Organization oluşturuluyor. Name: {Name}, LegalName: {LegalName}, TaxNumber: {TaxNumber}",
            dto.Name, dto.LegalName, dto.TaxNumber);

        var organization = new OrganizationEntity
        {
            Name = dto.Name,
            LegalName = dto.LegalName,
            TaxNumber = dto.TaxNumber,
            RegistrationNumber = dto.RegistrationNumber,
            Email = dto.Email,
            Phone = dto.Phone,
            Website = dto.Website,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            Status = EntityStatus.Active,
            Settings = dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null
        };

        await _context.Set<OrganizationEntity>().AddAsync(organization, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with all includes in one query (N+1 fix)
        organization = await _context.Set<OrganizationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organization.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Organization oluşturuldu. OrganizationId: {OrganizationId}, Name: {Name}",
            organization!.Id, organization.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<OrganizationDto>(organization);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<OrganizationDto?> GetOrganizationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        var organization = await _context.Set<OrganizationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (organization == null) return null;

        // ✅ PERFORMANCE: Batch loading - UserCount ve TeamCount'u toplu olarak yükle (N+1 fix)
        var userCount = await _context.Users
            .AsNoTracking()
            .CountAsync(u => u.OrganizationId == organization.Id, cancellationToken);

        var teamCount = await _context.Set<Team>()
            .AsNoTracking()
            .CountAsync(t => t.OrganizationId == organization.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<OrganizationDto>(organization);
        dto.UserCount = userCount;
        dto.TeamCount = teamCount;
        return dto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<OrganizationDto>> GetAllOrganizationsAsync(string? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        var query = _context.Set<OrganizationEntity>()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<EntityStatus>(status, true, out var statusEnum))
            {
                query = query.Where(o => o.Status == statusEnum);
            }
        }

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var totalCount = await query.CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch loading - Önce organization ID'lerini database'de al (ToListAsync() sonrası Select() YASAK)
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
        
        // ✅ PERFORMANCE: Batch loading - Tüm organization'lar için UserCount ve TeamCount'u toplu olarak yükle (N+1 fix)
        var userCounts = await _context.Users
            .AsNoTracking()
            .Where(u => organizationIds.Contains(u.OrganizationId ?? Guid.Empty))
            .GroupBy(u => u.OrganizationId)
            .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OrganizationId ?? Guid.Empty, x => x.Count, cancellationToken);

        var teamCounts = await _context.Set<Team>()
            .AsNoTracking()
            .Where(t => organizationIds.Contains(t.OrganizationId))
            .GroupBy(t => t.OrganizationId)
            .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OrganizationId, x => x.Count, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var result = _mapper.Map<IEnumerable<OrganizationDto>>(organizations).ToList();
        
        // ✅ PERFORMANCE: Dictionary'den Count'ları set et (memory'de minimal işlem)
        foreach (var dto in result)
        {
            dto.UserCount = userCounts.TryGetValue(dto.Id, out var uc) ? uc : 0;
            dto.TeamCount = teamCounts.TryGetValue(dto.Id, out var tc) ? tc : 0;
        }

        return new PagedResult<OrganizationDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateOrganizationAsync(Guid id, UpdateOrganizationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var organization = await _context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (organization == null) return false;

        if (!string.IsNullOrEmpty(dto.Name))
            organization.Name = dto.Name;
        if (dto.LegalName != null)
            organization.LegalName = dto.LegalName;
        if (dto.TaxNumber != null)
            organization.TaxNumber = dto.TaxNumber;
        if (dto.RegistrationNumber != null)
            organization.RegistrationNumber = dto.RegistrationNumber;
        if (dto.Email != null)
            organization.Email = dto.Email;
        if (dto.Phone != null)
            organization.Phone = dto.Phone;
        if (dto.Website != null)
            organization.Website = dto.Website;
        if (dto.Address != null)
            organization.Address = dto.Address;
        if (dto.City != null)
            organization.City = dto.City;
        if (dto.State != null)
            organization.State = dto.State;
        if (dto.PostalCode != null)
            organization.PostalCode = dto.PostalCode;
        if (dto.Country != null)
            organization.Country = dto.Country;
        if (!string.IsNullOrEmpty(dto.Status))
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<EntityStatus>(dto.Status, true, out var statusEnum))
            {
                organization.Status = statusEnum;
            }
        if (dto.Settings != null)
            organization.Settings = JsonSerializer.Serialize(dto.Settings);

        organization.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteOrganizationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var organization = await _context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (organization == null) return false;

        organization.IsDeleted = true;
        organization.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> VerifyOrganizationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var organization = await _context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (organization == null) return false;

        organization.IsVerified = true;
        organization.VerifiedAt = DateTime.UtcNow;
        organization.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> SuspendOrganizationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var organization = await _context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (organization == null) return false;

        organization.Status = EntityStatus.Suspended;
        organization.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<TeamDto> CreateTeamAsync(CreateTeamDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Team oluşturuluyor. OrganizationId: {OrganizationId}, Name: {Name}",
            dto.OrganizationId, dto.Name);

        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var organization = await _context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == dto.OrganizationId, cancellationToken);

        if (organization == null)
        {
            throw new NotFoundException("Organizasyon", dto.OrganizationId);
        }

        var team = new Team
        {
            OrganizationId = dto.OrganizationId,
            Name = dto.Name,
            Description = dto.Description,
            TeamLeadId = dto.TeamLeadId,
            IsActive = true,
            Settings = dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null
        };

        await _context.Set<Team>().AddAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with all includes in one query (N+1 fix)
        team = await _context.Set<Team>()
            .AsNoTracking()
            .Include(t => t.Organization)
            .Include(t => t.TeamLead)
            .FirstOrDefaultAsync(t => t.Id == team.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var teamDto = _mapper.Map<TeamDto>(team!);
        
        // ✅ PERFORMANCE: Batch loading - MemberCount'u yükle (N+1 fix)
        var memberCount = await _context.Set<TeamMember>()
            .AsNoTracking()
            .CountAsync(tm => tm.TeamId == team!.Id && tm.IsActive, cancellationToken);
        
        teamDto.MemberCount = memberCount;

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Team oluşturuldu. TeamId: {TeamId}, OrganizationId: {OrganizationId}, Name: {Name}",
            team.Id, dto.OrganizationId, dto.Name);

        return teamDto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<TeamDto?> GetTeamByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var team = await _context.Set<Team>()
            .AsNoTracking()
            .Include(t => t.Organization)
            .Include(t => t.TeamLead)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team == null) return null;

        // ✅ PERFORMANCE: Batch loading - MemberCount'u yükle (N+1 fix)
        var memberCount = await _context.Set<TeamMember>()
            .AsNoTracking()
            .CountAsync(tm => tm.TeamId == team.Id && tm.IsActive, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<TeamDto>(team);
        dto.MemberCount = memberCount;
        return dto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<TeamDto>> GetOrganizationTeamsAsync(Guid organizationId, bool? isActive = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var query = _context.Set<Team>()
            .AsNoTracking()
            .Include(t => t.Organization)
            .Include(t => t.TeamLead)
            .Where(t => t.OrganizationId == organizationId);

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var totalCount = await query.CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch loading - Önce team ID'lerini database'de al (ToListAsync() sonrası Select() YASAK)
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
        
        // ✅ PERFORMANCE: Batch loading - Tüm team'ler için MemberCount'u toplu olarak yükle (N+1 fix)
        var memberCounts = await _context.Set<TeamMember>()
            .AsNoTracking()
            .Where(tm => teamIds.Contains(tm.TeamId) && tm.IsActive)
            .GroupBy(tm => tm.TeamId)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeamId, x => x.Count, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var result = _mapper.Map<IEnumerable<TeamDto>>(teams).ToList();
        
        // ✅ PERFORMANCE: Dictionary'den Count'ları set et (memory'de minimal işlem)
        foreach (var dto in result)
        {
            dto.MemberCount = memberCounts.TryGetValue(dto.Id, out var count) ? count : 0;
        }

        return new PagedResult<TeamDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateTeamAsync(Guid id, UpdateTeamDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var team = await _context.Set<Team>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team == null) return false;

        if (!string.IsNullOrEmpty(dto.Name))
            team.Name = dto.Name;
        if (dto.Description != null)
            team.Description = dto.Description;
        if (dto.TeamLeadId.HasValue)
            team.TeamLeadId = dto.TeamLeadId.Value;
        if (dto.IsActive.HasValue)
            team.IsActive = dto.IsActive.Value;
        if (dto.Settings != null)
            team.Settings = JsonSerializer.Serialize(dto.Settings);

        team.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteTeamAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var team = await _context.Set<Team>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team == null) return false;

        team.IsDeleted = true;
        team.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<TeamMemberDto> AddTeamMemberAsync(Guid teamId, AddTeamMemberDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Team member ekleniyor. TeamId: {TeamId}, UserId: {UserId}, Role: {Role}",
            teamId, dto.UserId, dto.Role);

        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var team = await _context.Set<Team>()
            .FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);

        if (team == null)
        {
            throw new NotFoundException("Takım", teamId);
        }

        // ✅ PERFORMANCE: Removed manual !u.IsDeleted (Global Query Filter)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", dto.UserId);
        }

        // ✅ PERFORMANCE: Removed manual !tm.IsDeleted (Global Query Filter)
        // Check if user is already a member
        var existing = await _context.Set<TeamMember>()
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == dto.UserId, cancellationToken);

        if (existing != null)
        {
            throw new BusinessException("Kullanıcı zaten bu takımın üyesi.");
        }

        var teamMember = new TeamMember
        {
            TeamId = teamId,
            UserId = dto.UserId,
            Role = dto.Role,
            IsActive = true
        };

        await _context.Set<TeamMember>().AddAsync(teamMember, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with all includes in one query (N+1 fix)
        teamMember = await _context.Set<TeamMember>()
            .AsNoTracking()
            .Include(tm => tm.Team)
            .Include(tm => tm.User)
            .FirstOrDefaultAsync(tm => tm.Id == teamMember.Id, cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Team member eklendi. TeamMemberId: {TeamMemberId}, TeamId: {TeamId}, UserId: {UserId}",
            teamMember!.Id, teamId, dto.UserId);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<TeamMemberDto>(teamMember);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> RemoveTeamMemberAsync(Guid teamId, Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !tm.IsDeleted (Global Query Filter)
        var teamMember = await _context.Set<TeamMember>()
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId, cancellationToken);

        if (teamMember == null) return false;

        teamMember.IsDeleted = true;
        teamMember.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateTeamMemberAsync(Guid teamId, Guid userId, UpdateTeamMemberDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !tm.IsDeleted (Global Query Filter)
        var teamMember = await _context.Set<TeamMember>()
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId, cancellationToken);

        if (teamMember == null) return false;

        if (!string.IsNullOrEmpty(dto.Role))
            teamMember.Role = dto.Role;
        if (dto.IsActive.HasValue)
            teamMember.IsActive = dto.IsActive.Value;

        teamMember.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<TeamMemberDto>> GetTeamMembersAsync(Guid teamId, bool? isActive = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !tm.IsDeleted (Global Query Filter)
        var query = _context.Set<TeamMember>()
            .AsNoTracking()
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

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası foreach içinde MapToTeamMemberDto YASAK - AutoMapper kullan
        var dtos = _mapper.Map<IEnumerable<TeamMemberDto>>(members);

        return new PagedResult<TeamMemberDto>
        {
            Items = dtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<TeamDto>> GetUserTeamsAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !tm.IsDeleted and !tm.Team.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Batch loading - Önce team ID'lerini database'de al (ToListAsync() sonrası Select() YASAK)
        var teamIdsQuery = _context.Set<TeamMember>()
            .AsNoTracking()
            .Where(tm => tm.UserId == userId && tm.IsActive)
            .Select(tm => tm.TeamId)
            .Distinct();

        var totalCount = await teamIdsQuery.CountAsync(cancellationToken);

        var teamIds = await teamIdsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var teamMembers = await _context.Set<TeamMember>()
            .AsNoTracking()
            .Include(tm => tm.Team)
                .ThenInclude(t => t.Organization)
            .Include(tm => tm.Team)
                .ThenInclude(t => t.TeamLead)
            .Where(tm => tm.UserId == userId && tm.IsActive && teamIds.Contains(tm.TeamId))
            .Select(tm => tm.Team)
            .Distinct()
            .ToListAsync(cancellationToken);
        
        // ✅ PERFORMANCE: Batch loading - Tüm team'ler için MemberCount'u toplu olarak yükle (N+1 fix)
        var memberCounts = await _context.Set<TeamMember>()
            .AsNoTracking()
            .Where(tm => teamIds.Contains(tm.TeamId) && tm.IsActive)
            .GroupBy(tm => tm.TeamId)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeamId, x => x.Count, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var result = _mapper.Map<IEnumerable<TeamDto>>(teamMembers).ToList();
        
        // ✅ PERFORMANCE: Dictionary'den Count'ları set et (memory'de minimal işlem)
        foreach (var dto in result)
        {
            dto.MemberCount = memberCounts.TryGetValue(dto.Id, out var count) ? count : 0;
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

