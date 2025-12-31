using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.Governance;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Governance;


namespace Merge.Application.Services.Governance;

public class PolicyService : IPolicyService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PolicyService(ApplicationDbContext context, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto dto, Guid createdByUserId)
    {
        var policy = new Policy
        {
            PolicyType = dto.PolicyType,
            Title = dto.Title,
            Content = dto.Content,
            Version = dto.Version,
            IsActive = dto.IsActive,
            RequiresAcceptance = dto.RequiresAcceptance,
            EffectiveDate = dto.EffectiveDate ?? DateTime.UtcNow,
            ExpiryDate = dto.ExpiryDate,
            CreatedByUserId = createdByUserId,
            ChangeLog = dto.ChangeLog,
            Language = dto.Language
        };

        // If activating a new version, deactivate old versions of the same type
        if (dto.IsActive)
        {
            // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
            var existingPolicies = await _context.Set<Policy>()
                .Where(p => p.PolicyType == dto.PolicyType && 
                       p.Language == dto.Language && 
                       p.IsActive)
                .ToListAsync();

            foreach (var existing in existingPolicies)
            {
                existing.IsActive = false;
                existing.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.Set<Policy>().AddAsync(policy);
        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: Reload with Include (LoadAsync YASAK - N+1 Query)
        var reloadedPolicy = await _context.Set<Policy>()
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == policy.Id);

        if (reloadedPolicy == null)
        {
            throw new NotFoundException("Policy", policy.Id);
        }
        
        var policyDto = _mapper.Map<PolicyDto>(reloadedPolicy);
        policyDto.AcceptanceCount = await GetAcceptanceCountAsync(reloadedPolicy.Id);
        return policyDto;
    }

    public async Task<PolicyDto?> GetPolicyAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var policy = await _context.Set<Policy>()
            .AsNoTracking()
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (policy == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<PolicyDto>(policy);
        
        // ✅ PERFORMANCE: AcceptanceCount database'de hesapla
        dto.AcceptanceCount = await GetAcceptanceCountAsync(policy.Id);
        
        return dto;
    }

    public async Task<PolicyDto?> GetActivePolicyAsync(string policyType, string language = "tr")
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
            .FirstOrDefaultAsync();

        if (policy == null) return null;

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<PolicyDto>(policy);
        
        // ✅ PERFORMANCE: AcceptanceCount database'de hesapla
        dto.AcceptanceCount = await GetAcceptanceCountAsync(policy.Id);
        
        return dto;
    }

    public async Task<IEnumerable<PolicyDto>> GetPoliciesAsync(string? policyType = null, string? language = null, bool activeOnly = false)
    {
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

        var policies = await query
            .OrderByDescending(p => p.Version)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();

        // ✅ PERFORMANCE: Batch loading - tüm policy'ler için acceptanceCount'ları tek query'de al
        var policyIds = policies.Select(p => p.Id).ToList();
        var acceptanceCounts = await _context.Set<PolicyAcceptance>()
            .AsNoTracking()
            .Where(pa => policyIds.Contains(pa.PolicyId) && pa.IsActive)
            .GroupBy(pa => pa.PolicyId)
            .Select(g => new { PolicyId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PolicyId, x => x.Count);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var result = new List<PolicyDto>();
        foreach (var policy in policies)
        {
            var dto = _mapper.Map<PolicyDto>(policy);
            dto.AcceptanceCount = acceptanceCounts.GetValueOrDefault(policy.Id, 0);
            result.Add(dto);
        }
        return result;
    }

    public async Task<PolicyDto> UpdatePolicyAsync(Guid id, UpdatePolicyDto dto, Guid updatedByUserId)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var policy = await _context.Set<Policy>()
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (policy == null)
        {
            throw new NotFoundException("Politika", id);
        }

        if (!string.IsNullOrEmpty(dto.Title))
            policy.Title = dto.Title;
        if (!string.IsNullOrEmpty(dto.Content))
            policy.Content = dto.Content;
        if (!string.IsNullOrEmpty(dto.Version))
            policy.Version = dto.Version;
        if (dto.IsActive.HasValue)
            policy.IsActive = dto.IsActive.Value;
        if (dto.RequiresAcceptance.HasValue)
            policy.RequiresAcceptance = dto.RequiresAcceptance.Value;
        if (dto.EffectiveDate.HasValue)
            policy.EffectiveDate = dto.EffectiveDate.Value;
        if (dto.ExpiryDate.HasValue)
            policy.ExpiryDate = dto.ExpiryDate.Value;
        if (dto.ChangeLog != null)
            policy.ChangeLog = dto.ChangeLog;

        policy.CreatedByUserId = updatedByUserId;
        policy.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var policyDto = _mapper.Map<PolicyDto>(policy);
        policyDto.AcceptanceCount = await GetAcceptanceCountAsync(policy.Id);
        return policyDto;
    }

    public async Task<bool> DeletePolicyAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var policy = await _context.Set<Policy>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (policy == null) return false;

        policy.IsDeleted = true;
        policy.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ActivatePolicyAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var policy = await _context.Set<Policy>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (policy == null) return false;

        // Deactivate other versions of the same type
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var existingPolicies = await _context.Set<Policy>()
            .Where(p => p.PolicyType == policy.PolicyType && 
                   p.Language == policy.Language && 
                   p.IsActive &&
                   p.Id != id)
            .ToListAsync();

        foreach (var existing in existingPolicies)
        {
            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        policy.IsActive = true;
        policy.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivatePolicyAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var policy = await _context.Set<Policy>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (policy == null) return false;

        policy.IsActive = false;
        policy.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<PolicyAcceptanceDto> AcceptPolicyAsync(Guid userId, AcceptPolicyDto dto, string? ipAddress = null)
    {
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var policy = await _context.Set<Policy>()
            .FirstOrDefaultAsync(p => p.Id == dto.PolicyId && p.IsActive);

        if (policy == null)
        {
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
                                  pa.IsActive);

        if (existingAcceptance != null && existingAcceptance.IsActive)
        {
            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<PolicyAcceptanceDto>(existingAcceptance);
        }

        // Deactivate old acceptances for this policy
        // ✅ PERFORMANCE: Removed manual !pa.IsDeleted (Global Query Filter)
        var oldAcceptances = await _context.Set<PolicyAcceptance>()
            .Where(pa => pa.UserId == userId && pa.PolicyId == dto.PolicyId)
            .ToListAsync();

        foreach (var old in oldAcceptances)
        {
            old.IsActive = false;
            old.UpdatedAt = DateTime.UtcNow;
        }

        var acceptance = new PolicyAcceptance
        {
            PolicyId = dto.PolicyId,
            UserId = userId,
            AcceptedVersion = policy.Version,
            IpAddress = ipAddress ?? string.Empty,
            UserAgent = string.Empty,
            AcceptedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _context.Set<PolicyAcceptance>().AddAsync(acceptance);
        await _unitOfWork.SaveChangesAsync();

        // ✅ PERFORMANCE: Reload with Include (LoadAsync YASAK - N+1 Query)
        // ✅ FIX: SaveChangesAsync sonrası entity'yi yeniden yükle (tracking için)
        var reloadedAcceptance = await _context.Set<PolicyAcceptance>()
            .Include(pa => pa.Policy)
            .Include(pa => pa.User)
            .FirstOrDefaultAsync(pa => pa.Id == acceptance.Id);

        if (reloadedAcceptance == null)
        {
            throw new NotFoundException("Policy acceptance", acceptance.Id);
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<PolicyAcceptanceDto>(reloadedAcceptance);
    }

    public async Task<bool> RevokeAcceptanceAsync(Guid userId, Guid policyId)
    {
        // ✅ PERFORMANCE: Removed manual !pa.IsDeleted (Global Query Filter)
        var acceptance = await _context.Set<PolicyAcceptance>()
            .FirstOrDefaultAsync(pa => pa.UserId == userId && 
                                  pa.PolicyId == policyId && 
                                  pa.IsActive);

        if (acceptance == null) return false;

        acceptance.IsActive = false;
        acceptance.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<PolicyAcceptanceDto>> GetUserAcceptancesAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pa.IsDeleted (Global Query Filter)
        var acceptances = await _context.Set<PolicyAcceptance>()
            .AsNoTracking()
            .Include(pa => pa.Policy)
            .Include(pa => pa.User)
            .Where(pa => pa.UserId == userId)
            .OrderByDescending(pa => pa.AcceptedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select() YASAK - AutoMapper'ın Map<IEnumerable<T>> metodunu kullan
        return _mapper.Map<IEnumerable<PolicyAcceptanceDto>>(acceptances);
    }

    public async Task<bool> HasUserAcceptedAsync(Guid userId, string policyType, string version)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pa.IsDeleted (Global Query Filter)
        return await _context.Set<PolicyAcceptance>()
            .AsNoTracking()
            .Include(pa => pa.Policy)
            .AnyAsync(pa => pa.UserId == userId && 
                       pa.Policy.PolicyType == policyType && 
                       pa.AcceptedVersion == version && 
                       pa.IsActive);
    }

    public async Task<IEnumerable<PolicyDto>> GetPendingPoliciesAsync(Guid userId)
    {
        // ✅ PERFORMANCE: Database'de filtering yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: NOT EXISTS subquery kullan (memory'de işlem YASAK)
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
            .ToListAsync();

        // ✅ PERFORMANCE: Batch loading - tüm policy'ler için acceptanceCount'ları tek query'de al
        var policyIds = pendingPolicies.Select(p => p.Id).ToList();
        var acceptanceCounts = policyIds.Count > 0
            ? await _context.Set<PolicyAcceptance>()
                .AsNoTracking()
                .Where(pa => policyIds.Contains(pa.PolicyId) && pa.IsActive)
                .GroupBy(pa => pa.PolicyId)
                .Select(g => new { PolicyId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PolicyId, x => x.Count)
            : new Dictionary<Guid, int>();

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var result = new List<PolicyDto>();
        foreach (var policy in pendingPolicies)
        {
            var dto = _mapper.Map<PolicyDto>(policy);
            dto.AcceptanceCount = acceptanceCounts.GetValueOrDefault(policy.Id, 0);
            result.Add(dto);
        }
        return result;
    }

    public async Task<int> GetAcceptanceCountAsync(Guid policyId)
    {
        // ✅ PERFORMANCE: Removed manual !pa.IsDeleted (Global Query Filter)
        return await _context.Set<PolicyAcceptance>()
            .AsNoTracking()
            .CountAsync(pa => pa.PolicyId == policyId && pa.IsActive);
    }

    public async Task<Dictionary<string, int>> GetAcceptanceStatsAsync()
    {
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var policies = await _context.Set<Policy>()
            .AsNoTracking()
            .Select(p => new { p.Id, p.PolicyType, p.Version })
            .ToListAsync();

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
            .ToDictionaryAsync(x => x.PolicyId, x => x.Count);

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

