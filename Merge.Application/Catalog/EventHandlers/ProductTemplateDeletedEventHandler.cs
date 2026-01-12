using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Template Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductTemplateDeletedEventHandler : INotificationHandler<ProductTemplateDeletedEvent>
{
    private readonly ILogger<ProductTemplateDeletedEventHandler> _logger;

    public ProductTemplateDeletedEventHandler(ILogger<ProductTemplateDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductTemplateDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ProductTemplateDeletedEvent. TemplateId: {TemplateId}, Name: {Name}",
                notification.TemplateId, notification.Name);
            throw;
        }
    }
}
