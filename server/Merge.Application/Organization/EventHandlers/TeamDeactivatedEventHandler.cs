using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;


public class TeamDeactivatedEventHandler(ILogger<TeamDeactivatedEventHandler> logger) : INotificationHandler<TeamDeactivatedEvent>
{

    public async Task Handle(TeamDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Team deactivated event received. TeamId: {TeamId}, OrganizationId: {OrganizationId}, Name: {Name}",
            notification.TeamId, notification.OrganizationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Deactivation notification gönderimi
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
