using MediatR;

namespace Merge.Application.Identity.Commands.SendVerificationCode;

public record SendVerificationCodeCommand(
    Guid UserId,
    string Purpose = "Login") : IRequest<Unit>;

