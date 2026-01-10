using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// FlashSale Activated Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class FlashSaleActivatedEventHandler : INotificationHandler<FlashSaleActivatedEvent>
{
    private readonly ILogger<FlashSaleActivatedEventHandler> _logger;

    public FlashSaleActivatedEventHandler(ILogger<FlashSaleActivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(FlashSaleActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Flash sale activated event received. FlashSaleId: {FlashSaleId}, Title: {Title}",
            notification.FlashSaleId, notification.Title);

        // TODO: İleride burada şunlar yapılabilir:
        // - Push notification gönderimi (flash sale started)
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
