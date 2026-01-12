using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Catalog.EventHandlers;

/// <summary>
/// Product Template Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class ProductTemplateUpdatedEventHandler : INotificationHandler<ProductTemplateUpdatedEvent>
{
    private readonly ILogger<ProductTemplateUpdatedEventHandler> _logger;

    public ProductTemplateUpdatedEventHandler(ILogger<ProductTemplateUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(ProductTemplateUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Product template updated event received. TemplateId: {TemplateId}, Name: {Name}",
            notification.TemplateId, notification.Name);

        try
        {
            // TODO: İleride burada şunlar yapılabilir:
            // - Cache invalidation (product templates cache)
            // - Analytics tracking (template update metrics)
            // - External system integration
            // - Notification gönderimi (template kullanıcılarına güncelleme bildirimi)

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Error handling ProductTemplateUpdatedEvent. TemplateId: {TemplateId}, Name: {Name}",
                notification.TemplateId, notification.Name);
            throw;
        }
    }
}
