using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;


public class TeamMemberUpdatedEventHandler(ILogger<TeamMemberUpdatedEventHandler> logger) : INotificationHandler<TeamMemberUpdatedEvent>
{

    public async Task Handle(TeamMemberUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Team member updated event received. TeamMemberId: {TeamMemberId}, TeamId: {TeamId}, UserId: {UserId}, Role: {Role}",
            notification.TeamMemberId, notification.TeamId, notification.UserId, notification.Role);

        // TODO: İleride burada şunlar yapılabilir:
        // - Update notification gönderimi
        // - Analytics tracking
        // - Cache invalidation
        // - Permission update (if role changed)

        await Task.CompletedTask;
    }
}
