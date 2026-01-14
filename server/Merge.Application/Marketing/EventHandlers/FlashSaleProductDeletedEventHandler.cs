using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// FlashSaleProduct Deleted Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class FlashSaleProductDeletedEventHandler(ILogger<FlashSaleProductDeletedEventHandler> logger) : INotificationHandler<FlashSaleProductDeletedEvent>
{
    public async Task Handle(FlashSaleProductDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
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
