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

public class OrganizationService : IOrganizationService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrganizationService> _logger;
    private readonly PaginationSettings _paginationSettings;

    public OrganizationService(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OrganizationService> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Organization oluşturuluyor. Name: {Name}, LegalName: {LegalName}, TaxNumber: {TaxNumber}",
            dto.Name, dto.LegalName, dto.TaxNumber);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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
            dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null);

        await _context.Set<OrganizationEntity>().AddAsync(organization, cancellationToken);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
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
        // ✅ BOLUM 7.1: Records immutable olduğu için with expression kullan
        return dto with { UserCount = userCount, TeamCount = teamCount };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<OrganizationDto>> GetAllOrganizationsAsync(string? status = null, int page = 1, int pageSize = 0, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        if (pageSize <= 0) pageSize = _paginationSettings.DefaultPageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

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
        // ✅ BOLUM 7.1: Records immutable olduğu için with expression kullan
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateOrganizationAsync(Guid id, UpdateOrganizationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        var organization = await _context.Set<OrganizationEntity>()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (organization == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
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
            dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null);

        // Status update (separate domain method)
        if (!string.IsNullOrEmpty(dto.Status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<EntityStatus>(dto.Status, true, out var statusEnum))
            {
                if (statusEnum == EntityStatus.Active && organization.Status != EntityStatus.Active)
                    organization.Activate();
                else if (statusEnum == EntityStatus.Suspended && organization.Status != EntityStatus.Suspended)
                    organization.Suspend();
            }
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
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

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        organization.Delete();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
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

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        organization.Verify();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
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

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        organization.Suspend();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
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

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var team = Team.Create(
            dto.OrganizationId,
            dto.Name,
            dto.Description,
            dto.TeamLeadId,
            dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null);

        await _context.Set<Team>().AddAsync(team, cancellationToken);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        team = await _context.Set<Team>()
            .AsNoTracking()
            .Include(t => t.Organization)
            .Include(t => t.TeamLead)
            .FirstOrDefaultAsync(t => t.Id == team.Id, cancellationToken);

        if (team == null)
        {
            _logger.LogWarning("Team not found after creation");
            throw new NotFoundException("Team", Guid.Empty);
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var teamDto = _mapper.Map<TeamDto>(team);
        
        // ✅ PERFORMANCE: Batch loading - MemberCount'u yükle (N+1 fix)
        var memberCount = await _context.Set<TeamMember>()
            .AsNoTracking()
            .CountAsync(tm => tm.TeamId == team.Id && tm.IsActive, cancellationToken);
        
        // ✅ BOLUM 7.1: Records immutable olduğu için with expression kullan
        teamDto = teamDto with { MemberCount = memberCount };

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Team oluşturuldu. TeamId: {TeamId}, OrganizationId: {OrganizationId}, Name: {Name}",
            team.Id, dto.OrganizationId, dto.Name);

        return teamDto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<TeamDto?> GetTeamByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
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
        // ✅ BOLUM 7.1: Records immutable olduğu için with expression kullan
        return dto with { MemberCount = memberCount };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<TeamDto>> GetOrganizationTeamsAsync(Guid organizationId, bool? isActive = null, int page = 1, int pageSize = 0, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        if (pageSize <= 0) pageSize = _paginationSettings.DefaultPageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var query = _context.Set<Team>()
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
        // ✅ BOLUM 7.1: Records immutable olduğu için with expression kullan
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

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateTeamAsync(Guid id, UpdateTeamDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !t.IsDeleted (Global Query Filter)
        var team = await _context.Set<Team>()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (team == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        team.Update(
            dto.Name,
            dto.Description,
            dto.TeamLeadId,
            dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null);

        // IsActive update (separate domain method)
        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value && !team.IsActive)
                team.Activate();
            else if (!dto.IsActive.Value && team.IsActive)
                team.Deactivate();
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
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

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        team.Delete();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
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

        // Parse Role from string to enum
        if (!Enum.TryParse<TeamMemberRole>(dto.Role, true, out var role))
        {
            _logger.LogWarning("Invalid TeamMemberRole: {Role}, defaulting to Member", dto.Role);
            role = TeamMemberRole.Member;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var teamMember = TeamMember.Create(teamId, dto.UserId, role);

        // ✅ BOLUM 1.5: Domain Events - Team aggregate root'a event ekle
        team.AddDomainEvent(new TeamMemberAddedEvent(teamMember.Id, teamId, dto.UserId, role.ToString()));

        await _context.Set<TeamMember>().AddAsync(teamMember, cancellationToken);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
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
            .Include(tm => tm.Team)
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId, cancellationToken);

        if (teamMember == null) return false;

        // ✅ BOLUM 1.5: Domain Events - Team aggregate root'a event ekle
        if (teamMember.Team is Team team)
        {
            team.AddDomainEvent(new TeamMemberRemovedEvent(teamMember.Id, teamId, userId));
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        teamMember.Delete();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateTeamMemberAsync(Guid teamId, Guid userId, UpdateTeamMemberDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !tm.IsDeleted (Global Query Filter)
        var teamMember = await _context.Set<TeamMember>()
            .Include(tm => tm.Team)
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId, cancellationToken);

        if (teamMember == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (!string.IsNullOrEmpty(dto.Role))
        {
            if (Enum.TryParse<TeamMemberRole>(dto.Role, true, out var role))
            {
                teamMember.UpdateRole(role);
            }
            else
            {
                _logger.LogWarning("Invalid TeamMemberRole: {Role}", dto.Role);
            }
        }

        if (dto.IsActive.HasValue)
        {
            if (dto.IsActive.Value && !teamMember.IsActive)
                teamMember.Activate();
            else if (!dto.IsActive.Value && teamMember.IsActive)
                teamMember.Deactivate();
        }

        // ✅ BOLUM 1.5: Domain Events - Team aggregate root'a event ekle
        if (teamMember.Team is Team team)
        {
            team.AddDomainEvent(new TeamMemberUpdatedEvent(teamMember.Id, teamId, userId, teamMember.Role.ToString()));
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<TeamMemberDto>> GetTeamMembersAsync(Guid teamId, bool? isActive = null, int page = 1, int pageSize = 0, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        if (pageSize <= 0) pageSize = _paginationSettings.DefaultPageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !tm.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var query = _context.Set<TeamMember>()
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
    public async Task<PagedResult<TeamDto>> GetUserTeamsAsync(Guid userId, int page = 1, int pageSize = 0, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        if (pageSize <= 0) pageSize = _paginationSettings.DefaultPageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
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

        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var teamMembers = await _context.Set<TeamMember>()
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
        // ✅ BOLUM 7.1: Records immutable olduğu için with expression kullan
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

