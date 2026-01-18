using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class VolumeDiscountActivatedEventHandler(
    ILogger<VolumeDiscountActivatedEventHandler> logger) : INotificationHandler<VolumeDiscountActivatedEvent>
{

    public async Task Handle(VolumeDiscountActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Volume discount activated event received. VolumeDiscountId: {VolumeDiscountId}, ProductId: {ProductId}, CategoryId: {CategoryId}, OrganizationId: {OrganizationId}",
            notification.VolumeDiscountId, notification.ProductId, notification.CategoryId, notification.OrganizationId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation (product pricing cache)
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
