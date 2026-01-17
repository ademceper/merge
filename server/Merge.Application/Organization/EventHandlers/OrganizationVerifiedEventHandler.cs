using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Organization Verified Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class OrganizationVerifiedEventHandler(ILogger<OrganizationVerifiedEventHandler> logger) : INotificationHandler<OrganizationVerifiedEvent>
{

    public async Task Handle(OrganizationVerifiedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Organization verified event received. OrganizationId: {OrganizationId}, Name: {Name}",
            notification.OrganizationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Verification email gönderimi
        // - Analytics tracking
        // - Notification gönderimi (organization'a)

        await Task.CompletedTask;
    }
}
