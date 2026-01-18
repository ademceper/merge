using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;


public class OrganizationSuspendedEventHandler(ILogger<OrganizationSuspendedEventHandler> logger) : INotificationHandler<OrganizationSuspendedEvent>
{

    public async Task Handle(OrganizationSuspendedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Organization suspended event received. OrganizationId: {OrganizationId}, Name: {Name}",
            notification.OrganizationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Suspension notification gönderimi
        // - Analytics tracking
        // - External system sync (CRM, ERP)
        // - Access revocation (API keys, etc.)

        await Task.CompletedTask;
    }
}
