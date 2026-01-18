using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class AbandonedCartEmailCreatedEventHandler(ILogger<AbandonedCartEmailCreatedEventHandler> logger) : INotificationHandler<AbandonedCartEmailCreatedEvent>
{
    public async Task Handle(AbandonedCartEmailCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "AbandonedCartEmail created event received. EmailId: {EmailId}, CartId: {CartId}, UserId: {UserId}, EmailType: {EmailType}",
            notification.EmailId, notification.CartId, notification.UserId, notification.EmailType);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Email gönderimi

        await Task.CompletedTask;
    }
}
