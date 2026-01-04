using Merge.Application.DTOs.Governance;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Governance;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
public interface IPolicyService
{
    // Policy Management
    Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto dto, Guid createdByUserId, CancellationToken cancellationToken = default);
    Task<PolicyDto?> GetPolicyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PolicyDto?> GetActivePolicyAsync(string policyType, string language = "tr", CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    Task<PagedResult<PolicyDto>> GetPoliciesAsync(string? policyType = null, string? language = null, bool activeOnly = false, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PolicyDto> UpdatePolicyAsync(Guid id, UpdatePolicyDto dto, Guid updatedByUserId, CancellationToken cancellationToken = default);
    Task<bool> DeletePolicyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ActivatePolicyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeactivatePolicyAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Policy Acceptance
    Task<PolicyAcceptanceDto> AcceptPolicyAsync(Guid userId, AcceptPolicyDto dto, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<bool> RevokeAcceptanceAsync(Guid userId, Guid policyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PolicyAcceptanceDto>> GetUserAcceptancesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasUserAcceptedAsync(Guid userId, string policyType, string version, CancellationToken cancellationToken = default);
    Task<IEnumerable<PolicyDto>> GetPendingPoliciesAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Statistics
    Task<int> GetAcceptanceCountAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetAcceptanceStatsAsync(CancellationToken cancellationToken = default);
}

