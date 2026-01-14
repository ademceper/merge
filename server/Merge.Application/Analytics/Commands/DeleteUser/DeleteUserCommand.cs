using MediatR;

namespace Merge.Application.Analytics.Commands.DeleteUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteUserCommand(
    Guid UserId
) : IRequest<bool>;

