using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.User.EventHandlers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UserPreferenceUpdatedEventHandler(ILogger<UserPreferenceUpdatedEventHandler> logger) : INotificationHandler<UserPreferenceUpdatedEvent>
{

    public async Task Handle(UserPreferenceUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)

        logger.LogInformation(
            "User preference updated event received. UserPreferenceId: {UserPreferenceId}, UserId: {UserId}",
            notification.UserPreferenceId, notification.UserId);

                // - Analytics tracking (preference update metrics)
        // - Cache invalidation (user preferences cache)
        // - External system integration (user preference sync)
        // - Notification gönderimi (preferences changed)
        // - Theme/Language change handling (UI refresh)

        await Task.CompletedTask;
    }
}
