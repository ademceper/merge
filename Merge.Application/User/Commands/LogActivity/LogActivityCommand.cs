using MediatR;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Commands.LogActivity;

public record LogActivityCommand(
    Guid? UserId,
    string ActivityType,
    string EntityType,
    Guid? EntityId,
    string Description,
    string IpAddress,
    string UserAgent,
    string? Metadata,
    int DurationMs,
    bool WasSuccessful,
    string? ErrorMessage
) : IRequest;
