using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Marketing.EventHandlers;


public class FlashSaleActivatedEventHandler(ILogger<FlashSaleActivatedEventHandler> logger) : INotificationHandler<FlashSaleActivatedEvent>
{
    public async Task Handle(FlashSaleActivatedEvent notification, CancellationToken cancellationToken)
    {
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
