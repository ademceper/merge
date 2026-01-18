using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class WholesalePriceDeactivatedEventHandler(
    ILogger<WholesalePriceDeactivatedEventHandler> logger) : INotificationHandler<WholesalePriceDeactivatedEvent>
{

    public async Task Handle(WholesalePriceDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Wholesale price deactivated event received. WholesalePriceId: {WholesalePriceId}, ProductId: {ProductId}, OrganizationId: {OrganizationId}",
            notification.WholesalePriceId, notification.ProductId, notification.OrganizationId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation (product pricing cache)
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
