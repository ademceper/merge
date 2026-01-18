using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class FlashSaleProductDeletedEventHandler(ILogger<FlashSaleProductDeletedEventHandler> logger) : INotificationHandler<FlashSaleProductDeletedEvent>
{
    public async Task Handle(FlashSaleProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "FlashSaleProduct deleted event received. FlashSaleProductId: {FlashSaleProductId}, FlashSaleId: {FlashSaleId}, ProductId: {ProductId}",
            notification.FlashSaleProductId, notification.FlashSaleId, notification.ProductId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - Inventory sync

        await Task.CompletedTask;
    }
}
