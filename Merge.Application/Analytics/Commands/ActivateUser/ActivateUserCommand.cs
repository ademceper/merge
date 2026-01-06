using MediatR;

namespace Merge.Application.Analytics.Commands.ActivateUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ActivateUserCommand(
    Guid UserId
) : IRequest<bool>;

