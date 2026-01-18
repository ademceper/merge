using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class FlashSaleCreatedEventHandler(ILogger<FlashSaleCreatedEventHandler> logger) : INotificationHandler<FlashSaleCreatedEvent>
{
    public async Task Handle(FlashSaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Flash sale created event received. FlashSaleId: {FlashSaleId}, Title: {Title}, StartDate: {StartDate}, EndDate: {EndDate}",
            notification.FlashSaleId, notification.Title, notification.StartDate, notification.EndDate);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (flash sale created)
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
