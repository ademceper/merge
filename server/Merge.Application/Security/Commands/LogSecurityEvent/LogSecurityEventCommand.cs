using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Security.Commands.LogSecurityEvent;

public record LogSecurityEventCommand(
    Guid UserId,
    string EventType,
    string Severity,
    string? IpAddress = null,
    string? UserAgent = null,
    string? Location = null,
    string? DeviceFingerprint = null,
    bool IsSuspicious = false,
    SecurityEventMetadataDto? Details = null,
    bool RequiresAction = false
) : IRequest<AccountSecurityEventDto>;
