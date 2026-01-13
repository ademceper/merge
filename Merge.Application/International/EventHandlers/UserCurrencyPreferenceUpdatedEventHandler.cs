using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// UserCurrencyPreference Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class UserCurrencyPreferenceUpdatedEventHandler : INotificationHandler<UserCurrencyPreferenceUpdatedEvent>
{
    private readonly ILogger<UserCurrencyPreferenceUpdatedEventHandler> _logger;

    public UserCurrencyPreferenceUpdatedEventHandler(ILogger<UserCurrencyPreferenceUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCurrencyPreferenceUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling UserCurrencyPreferenceUpdatedEvent. PreferenceId: {PreferenceId}, UserId: {UserId}",
                notification.UserCurrencyPreferenceId, notification.UserId);
            throw;
        }
    }
}
