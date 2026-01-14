using MediatR;

namespace Merge.Application.Analytics.Commands.DeactivateUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeactivateUserCommand(
    Guid UserId
) : IRequest<bool>;

