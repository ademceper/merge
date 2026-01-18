using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;


public class ProductTemplateDeletedEventHandler(
    ILogger<ProductTemplateDeletedEventHandler> logger) : INotificationHandler<ProductTemplateDeletedEvent>
{

    public async Task Handle(ProductTemplateDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Product template deleted event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product templates cache)
            // - Analytics tracking (template deletion metrics)
            // - External system integration
            // - Cascade delete handling (products created from this template)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error handling ProductTemplateDeletedEvent. TemplateId: {TemplateId}, Name: {Name}",
                notification.TemplateId, notification.Name);
            throw;
        }
    }
}
