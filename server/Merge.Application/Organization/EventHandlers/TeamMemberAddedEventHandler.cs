using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;


public class TeamMemberAddedEventHandler(ILogger<TeamMemberAddedEventHandler> logger) : INotificationHandler<TeamMemberAddedEvent>
{

    public async Task Handle(TeamMemberAddedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Team member added event received. TeamMemberId: {TeamMemberId}, TeamId: {TeamId}, UserId: {UserId}, Role: {Role}",
            notification.TeamMemberId, notification.TeamId, notification.UserId, notification.Role);

        // TODO: İleride burada şunlar yapılabilir:
        // - Welcome notification gönderimi
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
