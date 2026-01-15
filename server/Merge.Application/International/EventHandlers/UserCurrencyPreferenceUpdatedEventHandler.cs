using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

public class UserCurrencyPreferenceUpdatedEventHandler(ILogger<UserCurrencyPreferenceUpdatedEventHandler> logger) : INotificationHandler<UserCurrencyPreferenceUpdatedEvent>
{
    public async Task Handle(UserCurrencyPreferenceUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "User currency preference updated event received. PreferenceId: {PreferenceId}, UserId: {UserId}, CurrencyId: {CurrencyId}, CurrencyCode: {CurrencyCode}",
            notification.UserCurrencyPreferenceId, notification.UserId, notification.CurrencyId, notification.CurrencyCode);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (user preferences cache)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling UserCurrencyPreferenceUpdatedEvent. PreferenceId: {PreferenceId}, UserId: {UserId}",
                notification.UserCurrencyPreferenceId, notification.UserId);
            throw;
        }
    }
}
