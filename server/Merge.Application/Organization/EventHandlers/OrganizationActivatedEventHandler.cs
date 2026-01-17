using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Organization Activated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class OrganizationActivatedEventHandler(ILogger<OrganizationActivatedEventHandler> logger) : INotificationHandler<OrganizationActivatedEvent>
{

    public async Task Handle(OrganizationActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Organization activated event received. OrganizationId: {OrganizationId}, Name: {Name}",
            notification.OrganizationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Activation notification gönderimi
        // - Analytics tracking
        // - External system sync (CRM, ERP)
        // - Access restoration (API keys, etc.)

        await Task.CompletedTask;
    }
}
