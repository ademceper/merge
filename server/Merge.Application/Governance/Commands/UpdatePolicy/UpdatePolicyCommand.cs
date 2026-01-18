using MediatR;
using Merge.Application.DTOs.Governance;

namespace Merge.Application.Governance.Commands.UpdatePolicy;

public record UpdatePolicyCommand(
    Guid Id,
    Guid UpdatedByUserId, // Controller'dan set edilecek
    string? Title = null,
    string? Content = null,
    string? Version = null,
    bool? IsActive = null,
    bool? RequiresAcceptance = null,
    DateTime? EffectiveDate = null,
    DateTime? ExpiryDate = null,
    string? ChangeLog = null,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<PolicyDto>;

