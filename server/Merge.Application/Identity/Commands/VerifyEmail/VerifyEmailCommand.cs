using MediatR;

namespace Merge.Application.Identity.Commands.VerifyEmail;

public record VerifyEmailCommand(
    string Token) : IRequest<Unit>;

