using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.International.EventHandlers;

/// <summary>
/// UserLanguagePreference Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class UserLanguagePreferenceUpdatedEventHandler : INotificationHandler<UserLanguagePreferenceUpdatedEvent>
{
    private readonly ILogger<UserLanguagePreferenceUpdatedEventHandler> _logger;

    public UserLanguagePreferenceUpdatedEventHandler(ILogger<UserLanguagePreferenceUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserLanguagePreferenceUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling UserLanguagePreferenceUpdatedEvent. PreferenceId: {PreferenceId}, UserId: {UserId}",
                notification.UserLanguagePreferenceId, notification.UserId);
            throw;
        }
    }
}
