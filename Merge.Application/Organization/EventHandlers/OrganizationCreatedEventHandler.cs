using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Organization Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class OrganizationCreatedEventHandler : INotificationHandler<OrganizationCreatedEvent>
{
    private readonly ILogger<OrganizationCreatedEventHandler> _logger;

    public OrganizationCreatedEventHandler(ILogger<OrganizationCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrganizationCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Organization created event received. OrganizationId: {OrganizationId}, Name: {Name}, Email: {Email}",
            notification.OrganizationId, notification.Name, notification.Email);

        // TODO: İleride burada şunlar yapılabilir:
        // - Welcome email gönderimi
        // - Analytics tracking
        // - Cache invalidation
        // - External system integration (CRM, ERP)
        // - Notification gönderimi (admin'lere)

        await Task.CompletedTask;
    }
}
