using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class FlashSaleDeletedEventHandler(ILogger<FlashSaleDeletedEventHandler> logger) : INotificationHandler<FlashSaleDeletedEvent>
{
    public async Task Handle(FlashSaleDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "FlashSale deleted event received. FlashSaleId: {FlashSaleId}, Title: {Title}",
            notification.FlashSaleId, notification.Title);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - External system sync

        await Task.CompletedTask;
    }
}
