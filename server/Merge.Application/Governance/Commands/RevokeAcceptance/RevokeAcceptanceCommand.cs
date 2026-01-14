using MediatR;

namespace Merge.Application.Governance.Commands.RevokeAcceptance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RevokeAcceptanceCommand(
    Guid UserId, // Controller'dan set edilecek
    Guid PolicyId
) : IRequest<bool>;

