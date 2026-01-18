using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.User.EventHandlers;

public class UserPreferenceCreatedEventHandler(ILogger<UserPreferenceCreatedEventHandler> logger) : INotificationHandler<UserPreferenceCreatedEvent>
{

    public async Task Handle(UserPreferenceCreatedEvent notification, CancellationToken cancellationToken)
    {

        logger.LogInformation(
            "User preference created event received. UserPreferenceId: {UserPreferenceId}, UserId: {UserId}",
            notification.UserPreferenceId, notification.UserId);

                // - Analytics tracking (preference creation metrics)
        // - Cache invalidation (user preferences cache)
        // - External system integration (user preference sync)
        // - Notification gönderimi (preferences initialized)

        await Task.CompletedTask;
    }
}
