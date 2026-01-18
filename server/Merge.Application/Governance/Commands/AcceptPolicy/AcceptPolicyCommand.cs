using MediatR;
using Merge.Application.DTOs.Governance;

namespace Merge.Application.Governance.Commands.AcceptPolicy;

public record AcceptPolicyCommand(
    Guid UserId, // Controller'dan set edilecek
    Guid PolicyId,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<PolicyAcceptanceDto>;

