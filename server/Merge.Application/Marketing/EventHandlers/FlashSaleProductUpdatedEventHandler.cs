using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class FlashSaleProductUpdatedEventHandler(ILogger<FlashSaleProductUpdatedEventHandler> logger) : INotificationHandler<FlashSaleProductUpdatedEvent>
{
    public async Task Handle(FlashSaleProductUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "FlashSaleProduct updated event received. FlashSaleProductId: {FlashSaleProductId}, FlashSaleId: {FlashSaleId}, ProductId: {ProductId}, UpdateType: {UpdateType}",
            notification.FlashSaleProductId, notification.FlashSaleId, notification.ProductId, notification.UpdateType);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - Price change notifications

        await Task.CompletedTask;
    }
}
