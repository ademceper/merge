using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// FlashSaleProduct Updated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class FlashSaleProductUpdatedEventHandler : INotificationHandler<FlashSaleProductUpdatedEvent>
{
    private readonly ILogger<FlashSaleProductUpdatedEventHandler> _logger;

    public FlashSaleProductUpdatedEventHandler(ILogger<FlashSaleProductUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(FlashSaleProductUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "FlashSaleProduct updated event received. FlashSaleProductId: {FlashSaleProductId}, FlashSaleId: {FlashSaleId}, ProductId: {ProductId}, UpdateType: {UpdateType}",
            notification.FlashSaleProductId, notification.FlashSaleId, notification.ProductId, notification.UpdateType);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - Price change notifications

        await Task.CompletedTask;
    }
}
