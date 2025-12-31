using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.Organization;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using System.Text.Json;
using OrganizationEntity = Merge.Domain.Entities.Organization;
using UserEntity = Merge.Domain.Entities.User;
using Merge.Application.DTOs.Organization;


namespace Merge.Application.Services.Organization;

public class OrganizationService : IOrganizationService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrganizationService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto)
    {
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
            Status = "Active",
            Settings = dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null
        };

        await _context.Set<OrganizationEntity>().AddAsync(organization);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with all includes in one query (N+1 fix)
        organization = await _context.Set<OrganizationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organization.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<OrganizationDto>(organization!);
    }

    public async Task<OrganizationDto?> GetOrganizationByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        var organization = await _context.Set<OrganizationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization == null) return null;

        // ✅ PERFORMANCE: Batch loading - UserCount ve TeamCount'u toplu olarak yükle (N+1 fix)
        var userCount = await _context.Users
            .AsNoTracking()
            .CountAsync(u => u.OrganizationId == organization.Id);

        var teamCount = await _context.Set<Team>()
            .AsNoTracking()
            .CountAsync(t => t.OrganizationId == organization.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<OrganizationDto>(organization);
        dto.UserCount = userCount;
        dto.TeamCount = teamCount;
        return dto;
    }

    public async Task<IEnumerable<OrganizationDto>> GetAllOrganizationsAsync(string? status = null)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        var query = _context.Set<OrganizationEntity>()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status);
        }

        // ✅ PERFORMANCE: Batch loading - Önce organization ID'lerini database'de al (ToListAsync() sonrası Select() YASAK)
        var organizationIds = await query
            .Select(o => o.Id)
            .ToListAsync();

        var organizations = await query
            .OrderBy(o => o.Name)
            .ToListAsync();
        
        // ✅ PERFORMANCE: Batch loading - Tüm organization'lar için UserCount ve TeamCount'u toplu olarak yükle (N+1 fix)
        var userCounts = await _context.Users
            .AsNoTracking()
            .Where(u => organizationIds.Contains(u.OrganizationId ?? Guid.Empty))
            .GroupBy(u => u.OrganizationId)
            .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OrganizationId ?? Guid.Empty, x => x.Count);

        var teamCounts = await _context.Set<Team>()
            .AsNoTracking()
            .Where(t => organizationIds.Contains(t.OrganizationId))
            .GroupBy(t => t.OrganizationId)
            .Select(g => new { OrganizationId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OrganizationId, x => x.Count);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var result = _mapper.Map<IEnumerable<OrganizationDto>>(organizations).ToList();
        
        // ✅ PERFORMANCE: Dictionary'den Count'ları set et (memory'de minimal işlem)
        foreach (var dto in result)
        {
            dto.UserCount = userCounts.TryGetValue(dto.Id, out var uc) ? uc : 0;
            dto.TeamCount = teamCounts.TryGetValue(dto.Id, out var tc) ? tc : 0;
        }

        return result;
    }

    public async Task<bool> UpdateOrganizationAsync(Guid id, UpdateOrganizationDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var organization = await _context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == id);

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
            organization.Status = dto.Status;
        if (dto.Settings != null)
            organization.Settings = JsonSerializer.Serialize(dto.Settings);

        organization.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteOrganizationAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var organization = await _context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization == null) return false;

        organization.IsDeleted = true;
        organization.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> VerifyOrganizationAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var organization = await _context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization == null) return false;

        organization.IsVerified = true;
        organization.VerifiedAt = DateTime.UtcNow;
        organization.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SuspendOrganizationAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var organization = await _context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization == null) return false;

        organization.Status = "Suspended";
        organization.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<TeamDto> CreateTeamAsync(CreateTeamDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var organization = await _context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == dto.OrganizationId);

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

        await _context.Set<Team>().AddAsync(team);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with all includes in one query (N+1 fix)
        team = await _context.Set<Team>()
            .AsNoTracking()
            .Include(t => t.Organization)
            .Include(t => t.TeamLead)
            .FirstOrDefaultAsync(t => t.Id == team.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var teamDto = _mapper.Map<TeamDto>(team!);
        
        // ✅ PERFORMANCE: Batch loading - MemberCount'u yükle (N+1 fix)
        var memberCount = await _context.Set<TeamMember>()
            .AsNoTracking()
            .CountAsync(tm => tm.TeamId == team!.Id && tm.IsActive);
        
        teamDto.MemberCount = memberCount;
        return teamDto;
    }

    public async Task<TeamDto?> GetTeamByIdAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var team = await _context.Set<Team>()
            .AsNoTracking()
            .Include(t => t.Organization)
            .Include(t => t.TeamLead)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null) return null;

        // ✅ PERFORMANCE: Batch loading - MemberCount'u yükle (N+1 fix)
        var memberCount = await _context.Set<TeamMember>()
            .AsNoTracking()
            .CountAsync(tm => tm.TeamId == team.Id && tm.IsActive);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<TeamDto>(team);
        dto.MemberCount = memberCount;
        return dto;
    }

    public async Task<IEnumerable<TeamDto>> GetOrganizationTeamsAsync(Guid organizationId, bool? isActive = null)
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

        // ✅ PERFORMANCE: Batch loading - Önce team ID'lerini database'de al (ToListAsync() sonrası Select() YASAK)
        var teamIds = await query
            .Select(t => t.Id)
            .ToListAsync();

        var teams = await query
            .OrderBy(t => t.Name)
            .ToListAsync();
        
        // ✅ PERFORMANCE: Batch loading - Tüm team'ler için MemberCount'u toplu olarak yükle (N+1 fix)
        var memberCounts = await _context.Set<TeamMember>()
            .AsNoTracking()
            .Where(tm => teamIds.Contains(tm.TeamId) && tm.IsActive)
            .GroupBy(tm => tm.TeamId)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeamId, x => x.Count);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var result = _mapper.Map<IEnumerable<TeamDto>>(teams).ToList();
        
        // ✅ PERFORMANCE: Dictionary'den Count'ları set et (memory'de minimal işlem)
        foreach (var dto in result)
        {
            dto.MemberCount = memberCounts.TryGetValue(dto.Id, out var count) ? count : 0;
        }

        return result;
    }

    public async Task<bool> UpdateTeamAsync(Guid id, UpdateTeamDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var team = await _context.Set<Team>()
            .FirstOrDefaultAsync(t => t.Id == id);

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
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteTeamAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var team = await _context.Set<Team>()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null) return false;

        team.IsDeleted = true;
        team.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<TeamMemberDto> AddTeamMemberAsync(Guid teamId, AddTeamMemberDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var team = await _context.Set<Team>()
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
        {
            throw new NotFoundException("Takım", teamId);
        }

        // ✅ PERFORMANCE: Removed manual !u.IsDeleted (Global Query Filter)
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == dto.UserId);

        if (user == null)
        {
            throw new NotFoundException("Kullanıcı", dto.UserId);
        }

        // ✅ PERFORMANCE: Removed manual !tm.IsDeleted (Global Query Filter)
        // Check if user is already a member
        var existing = await _context.Set<TeamMember>()
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == dto.UserId);

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

        await _context.Set<TeamMember>().AddAsync(teamMember);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with all includes in one query (N+1 fix)
        teamMember = await _context.Set<TeamMember>()
            .AsNoTracking()
            .Include(tm => tm.Team)
            .Include(tm => tm.User)
            .FirstOrDefaultAsync(tm => tm.Id == teamMember.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<TeamMemberDto>(teamMember!);
    }

    public async Task<bool> RemoveTeamMemberAsync(Guid teamId, Guid userId)
    {
        // ✅ PERFORMANCE: Removed manual !tm.IsDeleted (Global Query Filter)
        var teamMember = await _context.Set<TeamMember>()
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

        if (teamMember == null) return false;

        teamMember.IsDeleted = true;
        teamMember.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateTeamMemberAsync(Guid teamId, Guid userId, UpdateTeamMemberDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !tm.IsDeleted (Global Query Filter)
        var teamMember = await _context.Set<TeamMember>()
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

        if (teamMember == null) return false;

        if (!string.IsNullOrEmpty(dto.Role))
            teamMember.Role = dto.Role;
        if (dto.IsActive.HasValue)
            teamMember.IsActive = dto.IsActive.Value;

        teamMember.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<TeamMemberDto>> GetTeamMembersAsync(Guid teamId, bool? isActive = null)
    {
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

        var members = await query
            .OrderBy(tm => tm.Role)
            .ThenBy(tm => tm.User.FirstName)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası foreach içinde MapToTeamMemberDto YASAK - AutoMapper kullan
        return _mapper.Map<IEnumerable<TeamMemberDto>>(members);
    }

    public async Task<IEnumerable<TeamDto>> GetUserTeamsAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !tm.IsDeleted and !tm.Team.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Batch loading - Önce team ID'lerini database'de al (ToListAsync() sonrası Select() YASAK)
        var teamIds = await _context.Set<TeamMember>()
            .AsNoTracking()
            .Where(tm => tm.UserId == userId && tm.IsActive)
            .Select(tm => tm.TeamId)
            .Distinct()
            .ToListAsync();

        var teamMembers = await _context.Set<TeamMember>()
            .AsNoTracking()
            .Include(tm => tm.Team)
                .ThenInclude(t => t.Organization)
            .Include(tm => tm.Team)
                .ThenInclude(t => t.TeamLead)
            .Where(tm => tm.UserId == userId && tm.IsActive && teamIds.Contains(tm.TeamId))
            .Select(tm => tm.Team)
            .Distinct()
            .ToListAsync();
        
        // ✅ PERFORMANCE: Batch loading - Tüm team'ler için MemberCount'u toplu olarak yükle (N+1 fix)
        var memberCounts = await _context.Set<TeamMember>()
            .AsNoTracking()
            .Where(tm => teamIds.Contains(tm.TeamId) && tm.IsActive)
            .GroupBy(tm => tm.TeamId)
            .Select(g => new { TeamId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TeamId, x => x.Count);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var result = _mapper.Map<IEnumerable<TeamDto>>(teamMembers).ToList();
        
        // ✅ PERFORMANCE: Dictionary'den Count'ları set et (memory'de minimal işlem)
        foreach (var dto in result)
        {
            dto.MemberCount = memberCounts.TryGetValue(dto.Id, out var count) ? count : 0;
        }

        return result;
    }

}

