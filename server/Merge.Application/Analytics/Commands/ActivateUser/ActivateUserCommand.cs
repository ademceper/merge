using MediatR;

namespace Merge.Application.Analytics.Commands.ActivateUser;

public record ActivateUserCommand(
    Guid UserId
) : IRequest<bool>;

