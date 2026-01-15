using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class UserLanguagePreferenceUpdatedEventHandler(ILogger<UserLanguagePreferenceUpdatedEventHandler> logger) : INotificationHandler<UserLanguagePreferenceUpdatedEvent>
{
    public async Task Handle(UserLanguagePreferenceUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User language preference updated event received. PreferenceId: {PreferenceId}, UserId: {UserId}, LanguageId: {LanguageId}, LanguageCode: {LanguageCode}",
            notification.UserLanguagePreferenceId, notification.UserId, notification.LanguageId, notification.LanguageCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (user preferences cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling UserLanguagePreferenceUpdatedEvent. PreferenceId: {PreferenceId}, UserId: {UserId}",
                notification.UserLanguagePreferenceId, notification.UserId);
            throw;
        }
    }
}
