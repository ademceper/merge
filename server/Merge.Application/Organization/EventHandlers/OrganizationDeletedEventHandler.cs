using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Organization Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class OrganizationDeletedEventHandler : INotificationHandler<OrganizationDeletedEvent>
{
    private readonly ILogger<OrganizationDeletedEventHandler> _logger;

    public OrganizationDeletedEventHandler(ILogger<OrganizationDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrganizationDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Organization deleted event received. OrganizationId: {OrganizationId}, Name: {Name}",
            notification.OrganizationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Deletion notification gönderimi
        // - Analytics tracking
        // - External system sync (CRM, ERP)
        // - Data cleanup (if needed)
        // - Audit logging

        await Task.CompletedTask;
    }
}
