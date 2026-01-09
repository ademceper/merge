using MediatR;
using Merge.Application.DTOs.Governance;

namespace Merge.Application.Governance.Commands.CreatePolicy;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreatePolicyCommand(
    Guid CreatedByUserId, // Controller'dan set edilecek
    string PolicyType,
    string Title,
    string Content,
    string Version = "1.0",
    bool IsActive = true,
    bool RequiresAcceptance = true,
    DateTime? EffectiveDate = null,
    DateTime? ExpiryDate = null,
    string? ChangeLog = null,
    string Language = "tr"
) : IRequest<PolicyDto>;

