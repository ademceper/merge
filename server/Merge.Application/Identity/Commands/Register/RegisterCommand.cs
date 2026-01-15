using MediatR;
using Merge.Application.DTOs.Auth;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Identity.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PhoneNumber,
    string? IpAddress = null) : IRequest<AuthResponseDto>;

