using MediatR;
using Merge.Application.DTOs.Auth;

namespace Merge.Application.Identity.Commands.Login;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponseDto>;
