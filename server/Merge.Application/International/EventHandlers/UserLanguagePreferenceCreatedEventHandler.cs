using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class UserLanguagePreferenceCreatedEventHandler(ILogger<UserLanguagePreferenceCreatedEventHandler> logger) : INotificationHandler<UserLanguagePreferenceCreatedEvent>
{
    public async Task Handle(UserLanguagePreferenceCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
            logger.LogError(ex,
                "Error handling UserLanguagePreferenceCreatedEvent. PreferenceId: {PreferenceId}, UserId: {UserId}",
                notification.UserLanguagePreferenceId, notification.UserId);
            throw;
        }
    }
}
