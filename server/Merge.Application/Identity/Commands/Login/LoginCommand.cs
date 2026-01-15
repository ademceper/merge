using MediatR;
using Merge.Application.DTOs.Auth;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Identity.Commands.Login;

public record LoginCommand(
    string Email,
    string Password,
    string? IpAddress = null) : IRequest<AuthResponseDto>;
