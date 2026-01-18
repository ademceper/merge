using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.CreateB2BUser;

public record CreateB2BUserCommand(
    Guid UserId,
    Guid OrganizationId,
    string? EmployeeId,
    string? Department,
    string? JobTitle,
    decimal? CreditLimit,
    B2BUserSettingsDto? Settings
) : IRequest<B2BUserDto>;

