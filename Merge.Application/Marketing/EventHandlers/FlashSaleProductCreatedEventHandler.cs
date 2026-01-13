using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// FlashSaleProduct Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class FlashSaleProductCreatedEventHandler : INotificationHandler<FlashSaleProductCreatedEvent>
{
    private readonly ILogger<FlashSaleProductCreatedEventHandler> _logger;

    public FlashSaleProductCreatedEventHandler(ILogger<FlashSaleProductCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(FlashSaleProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "FlashSaleProduct created event received. FlashSaleProductId: {FlashSaleProductId}, FlashSaleId: {FlashSaleId}, ProductId: {ProductId}, SalePrice: {SalePrice}, StockLimit: {StockLimit}",
            notification.FlashSaleProductId, notification.FlashSaleId, notification.ProductId, notification.SalePrice, notification.StockLimit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - Inventory sync

        await Task.CompletedTask;
    }
}
