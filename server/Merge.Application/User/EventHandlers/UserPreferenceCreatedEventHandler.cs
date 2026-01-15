using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.User.EventHandlers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UserPreferenceCreatedEventHandler : INotificationHandler<UserPreferenceCreatedEvent>
{
    private readonly ILogger<UserPreferenceCreatedEventHandler> _logger;

    public UserPreferenceCreatedEventHandler(ILogger<UserPreferenceCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserPreferenceCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)

        _logger.LogInformation(
            "User preference created event received. UserPreferenceId: {UserPreferenceId}, UserId: {UserId}",
            notification.UserPreferenceId, notification.UserId);

                // - Analytics tracking (preference creation metrics)
        // - Cache invalidation (user preferences cache)
        // - External system integration (user preference sync)
        // - Notification gönderimi (preferences initialized)

        await Task.CompletedTask;
    }
}
