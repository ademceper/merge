using MediatR;

namespace Merge.Application.Governance.Commands.RevokeAcceptance;

public record RevokeAcceptanceCommand(
    Guid UserId, // Controller'dan set edilecek
    Guid PolicyId
) : IRequest<bool>;

