using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// FlashSale Deleted Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class FlashSaleDeletedEventHandler : INotificationHandler<FlashSaleDeletedEvent>
{
    private readonly ILogger<FlashSaleDeletedEventHandler> _logger;

    public FlashSaleDeletedEventHandler(ILogger<FlashSaleDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(FlashSaleDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "FlashSale deleted event received. FlashSaleId: {FlashSaleId}, Title: {Title}",
            notification.FlashSaleId, notification.Title);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - External system sync

        await Task.CompletedTask;
    }
}
