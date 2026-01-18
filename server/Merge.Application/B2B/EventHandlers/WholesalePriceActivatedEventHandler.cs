using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class WholesalePriceActivatedEventHandler(
    ILogger<WholesalePriceActivatedEventHandler> logger) : INotificationHandler<WholesalePriceActivatedEvent>
{

    public async Task Handle(WholesalePriceActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Wholesale price activated event received. WholesalePriceId: {WholesalePriceId}, ProductId: {ProductId}, OrganizationId: {OrganizationId}",
            notification.WholesalePriceId, notification.ProductId, notification.OrganizationId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation (product pricing cache)
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
