using MediatR;

namespace Merge.Application.Identity.Commands.ResendVerificationEmail;

public record ResendVerificationEmailCommand(
    Guid UserId) : IRequest<Unit>;

