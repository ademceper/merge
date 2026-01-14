using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// Wholesale Price Deactivated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class WholesalePriceDeactivatedEventHandler(
    ILogger<WholesalePriceDeactivatedEventHandler> logger) : INotificationHandler<WholesalePriceDeactivatedEvent>
{

    public async Task Handle(WholesalePriceDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Wholesale price deactivated event received. WholesalePriceId: {WholesalePriceId}, ProductId: {ProductId}, OrganizationId: {OrganizationId}",
            notification.WholesalePriceId, notification.ProductId, notification.OrganizationId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation (product pricing cache)
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
