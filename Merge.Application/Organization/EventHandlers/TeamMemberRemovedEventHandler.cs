using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Team Member Removed Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class TeamMemberRemovedEventHandler : INotificationHandler<TeamMemberRemovedEvent>
{
    private readonly ILogger<TeamMemberRemovedEventHandler> _logger;

    public TeamMemberRemovedEventHandler(ILogger<TeamMemberRemovedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TeamMemberRemovedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
