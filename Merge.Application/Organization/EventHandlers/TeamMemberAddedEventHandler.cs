using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Team Member Added Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class TeamMemberAddedEventHandler : INotificationHandler<TeamMemberAddedEvent>
{
    private readonly ILogger<TeamMemberAddedEventHandler> _logger;

    public TeamMemberAddedEventHandler(ILogger<TeamMemberAddedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TeamMemberAddedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Team member added event received. TeamMemberId: {TeamMemberId}, TeamId: {TeamId}, UserId: {UserId}, Role: {Role}",
            notification.TeamMemberId, notification.TeamId, notification.UserId, notification.Role);

        // TODO: İleride burada şunlar yapılabilir:
        // - Welcome notification gönderimi
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
