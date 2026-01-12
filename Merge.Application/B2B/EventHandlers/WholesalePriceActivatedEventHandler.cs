using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// Wholesale Price Activated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class WholesalePriceActivatedEventHandler : INotificationHandler<WholesalePriceActivatedEvent>
{
    private readonly ILogger<WholesalePriceActivatedEventHandler> _logger;

    public WholesalePriceActivatedEventHandler(ILogger<WholesalePriceActivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(WholesalePriceActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Wholesale price activated event received. WholesalePriceId: {WholesalePriceId}, ProductId: {ProductId}, OrganizationId: {OrganizationId}",
            notification.WholesalePriceId, notification.ProductId, notification.OrganizationId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation (product pricing cache)
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
