using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// UserLanguagePreference Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class UserLanguagePreferenceCreatedEventHandler : INotificationHandler<UserLanguagePreferenceCreatedEvent>
{
    private readonly ILogger<UserLanguagePreferenceCreatedEventHandler> _logger;

    public UserLanguagePreferenceCreatedEventHandler(ILogger<UserLanguagePreferenceCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserLanguagePreferenceCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "User language preference created event received. PreferenceId: {PreferenceId}, UserId: {UserId}, LanguageId: {LanguageId}, LanguageCode: {LanguageCode}",
            notification.UserLanguagePreferenceId, notification.UserId, notification.LanguageId, notification.LanguageCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (user preferences cache)
            // - Analytics tracking (language preference metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling UserLanguagePreferenceCreatedEvent. PreferenceId: {PreferenceId}, UserId: {UserId}",
                notification.UserLanguagePreferenceId, notification.UserId);
            throw;
        }
    }
}
