using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;


public class TeamUpdatedEventHandler(ILogger<TeamUpdatedEventHandler> logger) : INotificationHandler<TeamUpdatedEvent>
{

    public async Task Handle(TeamUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Team updated event received. TeamId: {TeamId}, OrganizationId: {OrganizationId}, Name: {Name}",
            notification.TeamId, notification.OrganizationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - Notification gönderimi (team members'a)

        await Task.CompletedTask;
    }
}
