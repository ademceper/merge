using MediatR;
using Merge.Application.DTOs.Governance;

namespace Merge.Application.Governance.Commands.AcceptPolicy;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AcceptPolicyCommand(
    Guid UserId, // Controller'dan set edilecek
    Guid PolicyId,
    string? IpAddress = null,
    string? UserAgent = null
) : IRequest<PolicyAcceptanceDto>;

