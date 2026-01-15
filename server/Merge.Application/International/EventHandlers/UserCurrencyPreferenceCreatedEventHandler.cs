using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class UserCurrencyPreferenceCreatedEventHandler(ILogger<UserCurrencyPreferenceCreatedEventHandler> logger) : INotificationHandler<UserCurrencyPreferenceCreatedEvent>
{
    public async Task Handle(UserCurrencyPreferenceCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User currency preference created event received. PreferenceId: {PreferenceId}, UserId: {UserId}, CurrencyId: {CurrencyId}, CurrencyCode: {CurrencyCode}",
            notification.UserCurrencyPreferenceId, notification.UserId, notification.CurrencyId, notification.CurrencyCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (user preferences cache)
            // - Analytics tracking (currency preference metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling UserCurrencyPreferenceCreatedEvent. PreferenceId: {PreferenceId}, UserId: {UserId}",
                notification.UserCurrencyPreferenceId, notification.UserId);
            throw;
        }
    }
}
