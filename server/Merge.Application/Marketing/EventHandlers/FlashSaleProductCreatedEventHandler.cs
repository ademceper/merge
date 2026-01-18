using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class FlashSaleProductCreatedEventHandler(ILogger<FlashSaleProductCreatedEventHandler> logger) : INotificationHandler<FlashSaleProductCreatedEvent>
{
    public async Task Handle(FlashSaleProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "FlashSaleProduct created event received. FlashSaleProductId: {FlashSaleProductId}, FlashSaleId: {FlashSaleId}, ProductId: {ProductId}, SalePrice: {SalePrice}, StockLimit: {StockLimit}",
            notification.FlashSaleProductId, notification.FlashSaleId, notification.ProductId, notification.SalePrice, notification.StockLimit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - Inventory sync

        await Task.CompletedTask;
    }
}
