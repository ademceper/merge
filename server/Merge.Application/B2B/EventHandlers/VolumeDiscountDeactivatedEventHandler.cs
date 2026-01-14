using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// Volume Discount Deactivated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class VolumeDiscountDeactivatedEventHandler(
    ILogger<VolumeDiscountDeactivatedEventHandler> logger) : INotificationHandler<VolumeDiscountDeactivatedEvent>
{

    public async Task Handle(VolumeDiscountDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Volume discount deactivated event received. VolumeDiscountId: {VolumeDiscountId}, ProductId: {ProductId}, CategoryId: {CategoryId}, OrganizationId: {OrganizationId}",
            notification.VolumeDiscountId, notification.ProductId, notification.CategoryId, notification.OrganizationId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation (product pricing cache)
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
