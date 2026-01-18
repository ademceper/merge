using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;


public class OrganizationVerifiedEventHandler(ILogger<OrganizationVerifiedEventHandler> logger) : INotificationHandler<OrganizationVerifiedEvent>
{

    public async Task Handle(OrganizationVerifiedEvent notification, CancellationToken cancellationToken)
    {
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
