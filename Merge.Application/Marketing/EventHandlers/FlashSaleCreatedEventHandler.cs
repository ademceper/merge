using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;

/// <summary>
/// FlashSale Created Event Handler - BOLUM 2.1.5: Domain Events Handler (ZORUNLU)
/// </summary>
public class FlashSaleCreatedEventHandler : INotificationHandler<FlashSaleCreatedEvent>
{
    private readonly ILogger<FlashSaleCreatedEventHandler> _logger;

    public FlashSaleCreatedEventHandler(ILogger<FlashSaleCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(FlashSaleCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Flash sale created event received. FlashSaleId: {FlashSaleId}, Title: {Title}, StartDate: {StartDate}, EndDate: {EndDate}",
            notification.FlashSaleId, notification.Title, notification.StartDate, notification.EndDate);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Notification gönderimi (flash sale created)
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
