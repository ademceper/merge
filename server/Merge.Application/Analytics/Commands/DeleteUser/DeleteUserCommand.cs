using MediatR;

namespace Merge.Application.Analytics.Commands.DeleteUser;

public record DeleteUserCommand(
    Guid UserId
) : IRequest<bool>;

