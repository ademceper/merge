using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;


public class OrganizationDeletedEventHandler(ILogger<OrganizationDeletedEventHandler> logger) : INotificationHandler<OrganizationDeletedEvent>
{

    public async Task Handle(OrganizationDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
