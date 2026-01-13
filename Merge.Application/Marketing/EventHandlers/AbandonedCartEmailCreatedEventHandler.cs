using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// AbandonedCartEmail Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class AbandonedCartEmailCreatedEventHandler : INotificationHandler<AbandonedCartEmailCreatedEvent>
{
    private readonly ILogger<AbandonedCartEmailCreatedEventHandler> _logger;

    public AbandonedCartEmailCreatedEventHandler(ILogger<AbandonedCartEmailCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(AbandonedCartEmailCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "AbandonedCartEmail created event received. EmailId: {EmailId}, CartId: {CartId}, UserId: {UserId}, EmailType: {EmailType}",
            notification.EmailId, notification.CartId, notification.UserId, notification.EmailType);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Email gönderimi

        await Task.CompletedTask;
    }
}
