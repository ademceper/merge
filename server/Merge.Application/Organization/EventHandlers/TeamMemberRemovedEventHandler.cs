using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;


public class TeamMemberRemovedEventHandler(ILogger<TeamMemberRemovedEventHandler> logger) : INotificationHandler<TeamMemberRemovedEvent>
{

    public async Task Handle(TeamMemberRemovedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Team member removed event received. TeamMemberId: {TeamMemberId}, TeamId: {TeamId}, UserId: {UserId}",
            notification.TeamMemberId, notification.TeamId, notification.UserId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Removal notification gönderimi
        // - Analytics tracking
        // - Cache invalidation
        // - Access revocation (if needed)

        await Task.CompletedTask;
    }
}
