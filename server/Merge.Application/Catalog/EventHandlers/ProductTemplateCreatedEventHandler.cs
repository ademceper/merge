using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class ProductTemplateCreatedEventHandler(
    ILogger<ProductTemplateCreatedEventHandler> logger) : INotificationHandler<ProductTemplateCreatedEvent>
{

    public async Task Handle(ProductTemplateCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product template created event received. TemplateId: {TemplateId}, Name: {Name}, CategoryId: {CategoryId}",
            notification.TemplateId, notification.Name, notification.CategoryId);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product templates cache)
            // - Analytics tracking (template creation metrics)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProductTemplateCreatedEvent. TemplateId: {TemplateId}, Name: {Name}",
                notification.TemplateId, notification.Name);
            throw;
        }
    }
}
