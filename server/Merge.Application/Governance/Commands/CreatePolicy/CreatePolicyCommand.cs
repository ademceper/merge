using MediatR;
using Merge.Application.DTOs.Governance;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Governance.Commands.CreatePolicy;

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

