using Merge.Application.DTOs.Governance;

namespace Merge.Application.Interfaces.Governance;

public interface IPolicyService
{
    // Policy Management
    Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto dto, Guid createdByUserId);
    Task<PolicyDto?> GetPolicyAsync(Guid id);
    Task<PolicyDto?> GetActivePolicyAsync(string policyType, string language = "tr");
    Task<IEnumerable<PolicyDto>> GetPoliciesAsync(string? policyType = null, string? language = null, bool activeOnly = false);
    Task<PolicyDto> UpdatePolicyAsync(Guid id, UpdatePolicyDto dto, Guid updatedByUserId);
    Task<bool> DeletePolicyAsync(Guid id);
    Task<bool> ActivatePolicyAsync(Guid id);
    Task<bool> DeactivatePolicyAsync(Guid id);
    
    // Policy Acceptance
    Task<PolicyAcceptanceDto> AcceptPolicyAsync(Guid userId, AcceptPolicyDto dto, string? ipAddress = null);
    Task<bool> RevokeAcceptanceAsync(Guid userId, Guid policyId);
    Task<IEnumerable<PolicyAcceptanceDto>> GetUserAcceptancesAsync(Guid userId);
    Task<bool> HasUserAcceptedAsync(Guid userId, string policyType, string version);
    Task<IEnumerable<PolicyDto>> GetPendingPoliciesAsync(Guid userId);
    
    // Statistics
    Task<int> GetAcceptanceCountAsync(Guid policyId);
    Task<Dictionary<string, int>> GetAcceptanceStatsAsync();
}

