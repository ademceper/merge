using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Template Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductTemplateCreatedEventHandler : INotificationHandler<ProductTemplateCreatedEvent>
{
    private readonly ILogger<ProductTemplateCreatedEventHandler> _logger;

    public ProductTemplateCreatedEventHandler(ILogger<ProductTemplateCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductTemplateCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ProductTemplateCreatedEvent. TemplateId: {TemplateId}, Name: {Name}",
                notification.TemplateId, notification.Name);
            throw;
        }
    }
}
