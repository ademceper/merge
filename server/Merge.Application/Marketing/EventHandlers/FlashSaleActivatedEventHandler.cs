using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// FlashSale Activated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
/// </summary>
public class FlashSaleActivatedEventHandler(ILogger<FlashSaleActivatedEventHandler> logger) : INotificationHandler<FlashSaleActivatedEvent>
{
    public async Task Handle(FlashSaleActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Flash sale activated event received. FlashSaleId: {FlashSaleId}, Title: {Title}",
            notification.FlashSaleId, notification.Title);

        // TODO: İleride burada şunlar yapılabilir:
        // - Push notification gönderimi (flash sale started)
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
