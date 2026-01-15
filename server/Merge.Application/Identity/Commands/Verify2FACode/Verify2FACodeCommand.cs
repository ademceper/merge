using MediatR;

namespace Merge.Application.Identity.Commands.Verify2FACode;

public record Verify2FACodeCommand(
    Guid UserId,
    string Code) : IRequest<bool>;

