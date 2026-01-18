using MediatR;

namespace Merge.Application.Analytics.Commands.DeactivateUser;

public record DeactivateUserCommand(
    Guid UserId
) : IRequest<bool>;

