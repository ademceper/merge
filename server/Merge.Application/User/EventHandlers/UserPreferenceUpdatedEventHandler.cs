using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.User.EventHandlers;

/// <summary>
/// UserPreference Updated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class UserPreferenceUpdatedEventHandler : INotificationHandler<UserPreferenceUpdatedEvent>
{
    private readonly ILogger<UserPreferenceUpdatedEventHandler> _logger;

    public UserPreferenceUpdatedEventHandler(ILogger<UserPreferenceUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserPreferenceUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "User preference updated event received. UserPreferenceId: {UserPreferenceId}, UserId: {UserId}",
            notification.UserPreferenceId, notification.UserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking (preference update metrics)
        // - Cache invalidation (user preferences cache)
        // - External system integration (user preference sync)
        // - Notification gönderimi (preferences changed)
        // - Theme/Language change handling (UI refresh)

        await Task.CompletedTask;
    }
}
