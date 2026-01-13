using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// UserCurrencyPreference Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class UserCurrencyPreferenceCreatedEventHandler : INotificationHandler<UserCurrencyPreferenceCreatedEvent>
{
    private readonly ILogger<UserCurrencyPreferenceCreatedEventHandler> _logger;

    public UserCurrencyPreferenceCreatedEventHandler(ILogger<UserCurrencyPreferenceCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCurrencyPreferenceCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling UserCurrencyPreferenceCreatedEvent. PreferenceId: {PreferenceId}, UserId: {UserId}",
                notification.UserCurrencyPreferenceId, notification.UserId);
            throw;
        }
    }
}
