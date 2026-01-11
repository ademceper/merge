using MediatR;
using Merge.Application.DTOs.Notification;

namespace Merge.Application.Notification.Commands.BulkUpdatePreferences;

/// <summary>
/// Bulk Update Preferences Command - BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public record BulkUpdatePreferencesCommand(
    Guid UserId,
    BulkUpdateNotificationPreferencesDto Dto) : IRequest<bool>;
