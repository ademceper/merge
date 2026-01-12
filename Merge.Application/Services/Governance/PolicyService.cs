using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Governance;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.Domain.Entities;
using Merge.Application.DTOs.Governance;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Governance;

// ⚠️ OBSOLETE: Bu service artık kullanılmamalı. MediatR Command/Query handler'ları kullanın.
[Obsolete("Use MediatR commands and queries instead. This service will be removed in a future version.")]
public class PolicyService : IPolicyService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PolicyService> _logger;

    public PolicyService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<PolicyService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    [Obsolete("Use CreatePolicyCommand via MediatR instead")]
    public async Task<PolicyDto> CreatePolicyAsync(object dtoObj, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not CreatePolicyDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }

        _logger.LogInformation("Policy olusturuluyor. PolicyType: {PolicyType}, Version: {Version}, CreatedByUserId: {CreatedByUserId}", 
            dto.PolicyType, dto.Version, createdByUserId);

        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var policy = Policy.Create(
                policyType: dto.PolicyType,
                title: dto.Title,
                content: dto.Content,
                version: dto.Version,
                createdByUserId: createdByUserId,
                isActive: dto.IsActive,
                requiresAcceptance: dto.RequiresAcceptance,
                effectiveDate: dto.EffectiveDate ?? DateTime.UtcNow,
                expiryDate: dto.ExpiryDate,
                changeLog: dto.ChangeLog,
                language: dto.Language);

            // If activating a new version, deactivate old versions of the same type
            if (dto.IsActive)
            {
                // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
                var existingPolicies = await _context.Set<Policy>()
                    .Where(p => p.PolicyType == dto.PolicyType && 
                           p.Language == dto.Language && 
                           p.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var existing in existingPolicies)
                {
                    // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                    existing.Deactivate();
                }
            }

            await _context.Set<Policy>().AddAsync(policy, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            // ✅ PERFORMANCE: Reload with Include (LoadAsync YASAK - N+1 Query)
            var reloadedPolicy = await _context.Set<Policy>()
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p => p.Id == policy.Id, cancellationToken);

            if (reloadedPolicy == null)
            {
                throw new NotFoundException("Policy", policy.Id);
            }
            
            var policyDto = _mapper.Map<PolicyDto>(reloadedPolicy);
            policyDto = policyDto with { AcceptanceCount = await GetAcceptanceCountAsync(reloadedPolicy.Id, cancellationToken) };

            _logger.LogInformation("Policy olusturuldu. PolicyId: {PolicyId}, PolicyType: {PolicyType}, Version: {Version}", 
                policy.Id, policy.PolicyType, policy.Version);

            return policyDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Policy olusturma hatasi. PolicyType: {PolicyType}, Version: {Version}, CreatedByUserId: {CreatedByUserId}", 
                dto.PolicyType, dto.Version, createdByUserId);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    [Obsolete("Use GetPolicyByIdQuery via MediatR instead")]
    public async Task<PolicyDto?> GetPolicyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var policy = await _context.Set<Policy>()
            .AsNoTracking()
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (policy == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<PolicyDto>(policy);
        
        // ✅ PERFORMANCE: AcceptanceCount database'de hesapla
        // ✅ BOLUM 7.1.5: Records - with expression kullanımı
        dto = dto with { AcceptanceCount = await GetAcceptanceCountAsync(policy.Id, cancellationToken) };
        
        return dto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    [Obsolete("Use GetActivePolicyQuery via MediatR instead")]
    public async Task<PolicyDto?> GetActivePolicyAsync(string policyType, string language = "tr", CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var policy = await _context.Set<Policy>()
            .AsNoTracking()
            .Include(p => p.CreatedBy)
            .Where(p => p.PolicyType == policyType && 
                   p.Language == language && 
                   p.IsActive &&
                   (p.EffectiveDate == null || p.EffectiveDate <= DateTime.UtcNow) &&
                   (p.ExpiryDate == null || p.ExpiryDate >= DateTime.UtcNow))
            .OrderByDescending(p => p.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (policy == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<PolicyDto>(policy);
        
        // ✅ PERFORMANCE: AcceptanceCount database'de hesapla
        // ✅ BOLUM 7.1.5: Records - with expression kullanımı
        dto = dto with { AcceptanceCount = await GetAcceptanceCountAsync(policy.Id, cancellationToken) };
        
        return dto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    [Obsolete("Use GetPoliciesQuery via MediatR instead")]
    public async Task<PagedResult<PolicyDto>> GetPoliciesAsync(string? policyType = null, string? language = null, bool activeOnly = false, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        // ✅ FIX: Explicitly type as IQueryable to avoid IIncludableQueryable type mismatch
        IQueryable<Policy> query = _context.Set<Policy>()
            .AsNoTracking()
            .Include(p => p.CreatedBy);

        if (!string.IsNullOrEmpty(policyType))
        {
            query = query.Where(p => p.PolicyType == policyType);
        }

        if (!string.IsNullOrEmpty(language))
        {
            query = query.Where(p => p.Language == language);
        }

        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var policies = await query
            .OrderByDescending(p => p.Version)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch loading - tüm policy'ler için acceptanceCount'ları tek query'de al
        var policyIds = policies.Select(p => p.Id).ToList();
        var acceptanceCounts = policyIds.Count > 0
            ? await _context.Set<PolicyAcceptance>()
                .AsNoTracking()
                .Where(pa => policyIds.Contains(pa.PolicyId) && pa.IsActive)
                .GroupBy(pa => pa.PolicyId)
                .Select(g => new { PolicyId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PolicyId, x => x.Count, cancellationToken)
            : new Dictionary<Guid, int>();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var result = new List<PolicyDto>(policies.Count);
        foreach (var policy in policies)
        {
            var dto = _mapper.Map<PolicyDto>(policy);
            // ✅ BOLUM 7.1.5: Records - with expression kullanımı
            dto = dto with { AcceptanceCount = acceptanceCounts.GetValueOrDefault(policy.Id, 0) };
            result.Add(dto);
        }

        return new PagedResult<PolicyDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    [Obsolete("Use UpdatePolicyCommand via MediatR instead")]
    public async Task<PolicyDto> UpdatePolicyAsync(Guid id, object dtoObj, Guid updatedByUserId, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not UpdatePolicyDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }

        _logger.LogInformation("Policy guncelleniyor. PolicyId: {PolicyId}, UpdatedByUserId: {UpdatedByUserId}", id, updatedByUserId);

        try
        {
            // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
            var policy = await _context.Set<Policy>()
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (policy == null)
            {
                _logger.LogWarning("Policy bulunamadi. PolicyId: {PolicyId}", id);
                throw new NotFoundException("Politika", id);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            if (!string.IsNullOrEmpty(dto.Title))
                policy.UpdateTitle(dto.Title);
            if (!string.IsNullOrEmpty(dto.Content))
                policy.UpdateContent(dto.Content);
            if (!string.IsNullOrEmpty(dto.Version))
                policy.UpdateVersion(dto.Version);
            if (dto.IsActive.HasValue)
            {
                if (dto.IsActive.Value)
                    policy.Activate();
                else
                    policy.Deactivate();
            }
            if (dto.RequiresAcceptance.HasValue)
                policy.UpdateRequiresAcceptance(dto.RequiresAcceptance.Value);
            if (dto.EffectiveDate.HasValue)
                policy.UpdateEffectiveDate(dto.EffectiveDate.Value);
            if (dto.ExpiryDate.HasValue)
                policy.UpdateExpiryDate(dto.ExpiryDate.Value);
            if (dto.ChangeLog != null)
                policy.UpdateChangeLog(dto.ChangeLog);

            // Update created by user ID
            policy.UpdateCreatedByUserId(updatedByUserId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            var policyDto = _mapper.Map<PolicyDto>(policy);
            // ✅ BOLUM 7.1.5: Records - with expression kullanımı
            policyDto = policyDto with { AcceptanceCount = await GetAcceptanceCountAsync(policy.Id, cancellationToken) };

            _logger.LogInformation("Policy guncellendi. PolicyId: {PolicyId}, Version: {Version}", policy.Id, policy.Version);

            return policyDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Policy guncelleme hatasi. PolicyId: {PolicyId}, UpdatedByUserId: {UpdatedByUserId}", id, updatedByUserId);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeletePolicyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var policy = await _context.Set<Policy>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (policy == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        policy.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    [Obsolete("Use ActivatePolicyCommand via MediatR instead")]
    public async Task<bool> ActivatePolicyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var policy = await _context.Set<Policy>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (policy == null) return false;

        // Deactivate other versions of the same type
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var existingPolicies = await _context.Set<Policy>()
            .Where(p => p.PolicyType == policy.PolicyType && 
                   p.Language == policy.Language && 
                   p.IsActive &&
                   p.Id != id)
            .ToListAsync(cancellationToken);

        foreach (var existing in existingPolicies)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            existing.Deactivate();
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        policy.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    [Obsolete("Use DeactivatePolicyCommand via MediatR instead")]
    public async Task<bool> DeactivatePolicyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var policy = await _context.Set<Policy>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (policy == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        policy.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 9.1: ILogger kullanimi (ZORUNLU)
    // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
    [Obsolete("Use AcceptPolicyCommand via MediatR instead")]
    public async Task<PolicyAcceptanceDto> AcceptPolicyAsync(Guid userId, object dtoObj, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        if (dtoObj is not AcceptPolicyDto dto)
        {
            throw new ArgumentException("Invalid DTO type", nameof(dtoObj));
        }

        _logger.LogInformation("Policy kabul ediliyor. UserId: {UserId}, PolicyId: {PolicyId}", userId, dto.PolicyId);

        try
        {
            // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
            var policy = await _context.Set<Policy>()
                .FirstOrDefaultAsync(p => p.Id == dto.PolicyId && p.IsActive, cancellationToken);

            if (policy == null)
            {
                _logger.LogWarning("Policy bulunamadi veya aktif degil. PolicyId: {PolicyId}", dto.PolicyId);
                throw new NotFoundException("Politika", dto.PolicyId);
            }

            // Check if user already accepted this version
            // ✅ PERFORMANCE: Removed manual !pa.IsDeleted (Global Query Filter)
            var existingAcceptance = await _context.Set<PolicyAcceptance>()
                .Include(pa => pa.Policy)
                .Include(pa => pa.User)
                .FirstOrDefaultAsync(pa => pa.UserId == userId && 
                                      pa.PolicyId == dto.PolicyId && 
                                      pa.AcceptedVersion == policy.Version &&
                                      pa.IsActive, cancellationToken);

            if (existingAcceptance != null && existingAcceptance.IsActive)
            {
                // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
                return _mapper.Map<PolicyAcceptanceDto>(existingAcceptance);
            }

            // Deactivate old acceptances for this policy
            // ✅ PERFORMANCE: Removed manual !pa.IsDeleted (Global Query Filter)
            var oldAcceptances = await _context.Set<PolicyAcceptance>()
                .Where(pa => pa.UserId == userId && pa.PolicyId == dto.PolicyId)
                .ToListAsync(cancellationToken);

            foreach (var old in oldAcceptances)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
                if (old.IsActive)
                {
                    old.Revoke();
                }
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var acceptance = PolicyAcceptance.Create(
                policyId: dto.PolicyId,
                userId: userId,
                acceptedVersion: policy.Version,
                ipAddress: ipAddress ?? string.Empty,
                userAgent: string.Empty);

            await _context.Set<PolicyAcceptance>().AddAsync(acceptance, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include (LoadAsync YASAK - N+1 Query)
            // ✅ FIX: SaveChangesAsync sonrası entity'yi yeniden yükle (tracking için)
            var reloadedAcceptance = await _context.Set<PolicyAcceptance>()
                .Include(pa => pa.Policy)
                .Include(pa => pa.User)
                .FirstOrDefaultAsync(pa => pa.Id == acceptance.Id, cancellationToken);

            if (reloadedAcceptance == null)
            {
                throw new NotFoundException("Policy acceptance", acceptance.Id);
            }

            _logger.LogInformation("Policy kabul edildi. AcceptanceId: {AcceptanceId}, UserId: {UserId}, PolicyId: {PolicyId}", 
                acceptance.Id, userId, dto.PolicyId);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<PolicyAcceptanceDto>(reloadedAcceptance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Policy kabul etme hatasi. UserId: {UserId}, PolicyId: {PolicyId}", userId, dto.PolicyId);
            throw; // ✅ BOLUM 2.1: Exception yutulmamali (ZORUNLU)
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    [Obsolete("Use RevokeAcceptanceCommand via MediatR instead")]
    public async Task<bool> RevokeAcceptanceAsync(Guid userId, Guid policyId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !pa.IsDeleted (Global Query Filter)
        var acceptance = await _context.Set<PolicyAcceptance>()
            .FirstOrDefaultAsync(pa => pa.UserId == userId && 
                                  pa.PolicyId == policyId && 
                                  pa.IsActive, cancellationToken);

        if (acceptance == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        acceptance.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    [Obsolete("Use GetUserAcceptancesQuery via MediatR instead")]
    public async Task<IEnumerable<PolicyAcceptanceDto>> GetUserAcceptancesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pa.IsDeleted (Global Query Filter)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var acceptances = await _context.Set<PolicyAcceptance>()
            .AsNoTracking()
            .Include(pa => pa.Policy)
            .Include(pa => pa.User)
            .Where(pa => pa.UserId == userId)
            .OrderByDescending(pa => pa.AcceptedAt)
            .Take(500) // ✅ Güvenlik: Maksimum 500 acceptance
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var result = new List<PolicyAcceptanceDto>(acceptances.Count);
        foreach (var acceptance in acceptances)
        {
            result.Add(_mapper.Map<PolicyAcceptanceDto>(acceptance));
        }
        return result;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    [Obsolete("This method is deprecated. Use GetUserAcceptancesQuery and filter in-memory if needed.")]
    public async Task<bool> HasUserAcceptedAsync(Guid userId, string policyType, string version, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pa.IsDeleted (Global Query Filter)
        return await _context.Set<PolicyAcceptance>()
            .AsNoTracking()
            .Include(pa => pa.Policy)
            .AnyAsync(pa => pa.UserId == userId && 
                       pa.Policy.PolicyType == policyType && 
                       pa.AcceptedVersion == version && 
                       pa.IsActive, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
    [Obsolete("Use GetPendingPoliciesQuery via MediatR instead")]
    public async Task<IEnumerable<PolicyDto>> GetPendingPoliciesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Database'de filtering yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: NOT EXISTS subquery kullan (memory'de işlem YASAK)
        // ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
        var pendingPolicies = await _context.Set<Policy>()
            .AsNoTracking()
            .Include(p => p.CreatedBy)
            .Where(p => p.IsActive && 
                   p.RequiresAcceptance &&
                   (p.EffectiveDate == null || p.EffectiveDate <= DateTime.UtcNow) &&
                   (p.ExpiryDate == null || p.ExpiryDate >= DateTime.UtcNow) &&
                   !_context.Set<PolicyAcceptance>()
                       .Any(pa => pa.UserId == userId && 
                                 pa.PolicyId == p.Id && 
                                 pa.AcceptedVersion == p.Version && 
                                 pa.IsActive))
            .OrderByDescending(p => p.Version)
            .Take(100) // ✅ Güvenlik: Maksimum 100 pending policy
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch loading - tüm policy'ler için acceptanceCount'ları tek query'de al
        var policyIds = pendingPolicies.Select(p => p.Id).ToList();
        var acceptanceCounts = policyIds.Count > 0
            ? await _context.Set<PolicyAcceptance>()
                .AsNoTracking()
                .Where(pa => policyIds.Contains(pa.PolicyId) && pa.IsActive)
                .GroupBy(pa => pa.PolicyId)
                .Select(g => new { PolicyId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PolicyId, x => x.Count, cancellationToken)
            : new Dictionary<Guid, int>();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var result = new List<PolicyDto>(pendingPolicies.Count);
        foreach (var policy in pendingPolicies)
        {
            var dto = _mapper.Map<PolicyDto>(policy);
            // ✅ BOLUM 7.1.5: Records - with expression kullanımı
            dto = dto with { AcceptanceCount = acceptanceCounts.GetValueOrDefault(policy.Id, 0) };
            result.Add(dto);
        }
        return result;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    [Obsolete("Use GetAcceptanceCountQuery via MediatR instead")]
    public async Task<int> GetAcceptanceCountAsync(Guid policyId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !pa.IsDeleted (Global Query Filter)
        return await _context.Set<PolicyAcceptance>()
            .AsNoTracking()
            .CountAsync(pa => pa.PolicyId == policyId && pa.IsActive, cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    [Obsolete("Use GetAcceptanceStatsQuery via MediatR instead")]
    public async Task<Dictionary<string, int>> GetAcceptanceStatsAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var policies = await _context.Set<Policy>()
            .AsNoTracking()
            .Select(p => new { p.Id, p.PolicyType, p.Version })
            .ToListAsync(cancellationToken);

        if (policies.Count == 0)
        {
            return new Dictionary<string, int>();
        }

        var policyIds = policies.Select(p => p.Id).ToList();
        
        // ✅ PERFORMANCE: Batch loading - tüm policy'ler için acceptanceCount'ları tek query'de al
        var acceptanceCounts = await _context.Set<PolicyAcceptance>()
            .AsNoTracking()
            .Where(pa => policyIds.Contains(pa.PolicyId) && pa.IsActive)
            .GroupBy(pa => pa.PolicyId)
            .Select(g => new { PolicyId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PolicyId, x => x.Count, cancellationToken);

        // ✅ PERFORMANCE: Memory'de minimal işlem (sadece dictionary oluşturma)
        var stats = new Dictionary<string, int>();
        foreach (var policy in policies)
        {
            var count = acceptanceCounts.GetValueOrDefault(policy.Id, 0);
            stats[$"{policy.PolicyType}_{policy.Version}"] = count;
        }

        return stats;
    }

}

