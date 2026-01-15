using MediatR;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Identity.Commands.RevokeToken;

public record RevokeTokenCommand(
    string RefreshToken,
    string? IpAddress) : IRequest<Unit>;

