using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Organization Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class OrganizationUpdatedEventHandler : INotificationHandler<OrganizationUpdatedEvent>
{
    private readonly ILogger<OrganizationUpdatedEventHandler> _logger;

    public OrganizationUpdatedEventHandler(ILogger<OrganizationUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrganizationUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Organization updated event received. OrganizationId: {OrganizationId}, Name: {Name}",
            notification.OrganizationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation
        // - Analytics tracking
        // - External system sync (CRM, ERP)

        await Task.CompletedTask;
    }
}
