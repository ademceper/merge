using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Team Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class TeamCreatedEventHandler(ILogger<TeamCreatedEventHandler> logger) : INotificationHandler<TeamCreatedEvent>
{

    public async Task Handle(TeamCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Team created event received. TeamId: {TeamId}, OrganizationId: {OrganizationId}, Name: {Name}",
            notification.TeamId, notification.OrganizationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - Notification gönderimi (team lead'e)

        await Task.CompletedTask;
    }
}
