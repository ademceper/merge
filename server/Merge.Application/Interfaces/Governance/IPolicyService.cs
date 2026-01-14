using Merge.Application.DTOs.Governance;
using Merge.Application.Common;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Interfaces.Governance;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
// ⚠️ OBSOLETE: Bu interface artık kullanılmamalı. MediatR Command/Query handler'ları kullanın.
[Obsolete("Use MediatR commands and queries instead. This service will be removed in a future version.")]
public interface IPolicyService
{
    // Policy Management
    [Obsolete("Use CreatePolicyCommand via MediatR instead")]
    Task<PolicyDto> CreatePolicyAsync(object dtoObj, Guid createdByUserId, CancellationToken cancellationToken = default);
    [Obsolete("Use GetPolicyByIdQuery via MediatR instead")]
    Task<PolicyDto?> GetPolicyAsync(Guid id, CancellationToken cancellationToken = default);
    [Obsolete("Use GetActivePolicyQuery via MediatR instead")]
    Task<PolicyDto?> GetActivePolicyAsync(string policyType, string language = "tr", CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    [Obsolete("Use GetPoliciesQuery via MediatR instead")]
    Task<PagedResult<PolicyDto>> GetPoliciesAsync(string? policyType = null, string? language = null, bool activeOnly = false, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    [Obsolete("Use UpdatePolicyCommand via MediatR instead")]
    Task<PolicyDto> UpdatePolicyAsync(Guid id, object dtoObj, Guid updatedByUserId, CancellationToken cancellationToken = default);
    [Obsolete("Use DeletePolicyCommand via MediatR instead")]
    Task<bool> DeletePolicyAsync(Guid id, CancellationToken cancellationToken = default);
    [Obsolete("Use ActivatePolicyCommand via MediatR instead")]
    Task<bool> ActivatePolicyAsync(Guid id, CancellationToken cancellationToken = default);
    [Obsolete("Use DeactivatePolicyCommand via MediatR instead")]
    Task<bool> DeactivatePolicyAsync(Guid id, CancellationToken cancellationToken = default);
    
    // Policy Acceptance
    [Obsolete("Use AcceptPolicyCommand via MediatR instead")]
    Task<PolicyAcceptanceDto> AcceptPolicyAsync(Guid userId, object dtoObj, string? ipAddress = null, CancellationToken cancellationToken = default);
    [Obsolete("Use RevokeAcceptanceCommand via MediatR instead")]
    Task<bool> RevokeAcceptanceAsync(Guid userId, Guid policyId, CancellationToken cancellationToken = default);
    [Obsolete("Use GetUserAcceptancesQuery via MediatR instead")]
    Task<IEnumerable<PolicyAcceptanceDto>> GetUserAcceptancesAsync(Guid userId, CancellationToken cancellationToken = default);
    [Obsolete("This method is deprecated. Use GetUserAcceptancesQuery and filter in-memory if needed.")]
    Task<bool> HasUserAcceptedAsync(Guid userId, string policyType, string version, CancellationToken cancellationToken = default);
    [Obsolete("Use GetPendingPoliciesQuery via MediatR instead")]
    Task<IEnumerable<PolicyDto>> GetPendingPoliciesAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // Statistics
    Task<int> GetAcceptanceCountAsync(Guid policyId, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetAcceptanceStatsAsync(CancellationToken cancellationToken = default);
}

