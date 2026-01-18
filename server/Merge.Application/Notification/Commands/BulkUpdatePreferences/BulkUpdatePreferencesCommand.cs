using MediatR;
using Merge.Application.DTOs.Notification;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.Notification.Commands.BulkUpdatePreferences;


public record BulkUpdatePreferencesCommand(
    Guid UserId,
    BulkUpdateNotificationPreferencesDto Dto) : IRequest<bool>;
