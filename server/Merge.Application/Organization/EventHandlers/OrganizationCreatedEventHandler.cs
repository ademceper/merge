using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;


public class OrganizationCreatedEventHandler(ILogger<OrganizationCreatedEventHandler> logger) : INotificationHandler<OrganizationCreatedEvent>
{

    public async Task Handle(OrganizationCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
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
