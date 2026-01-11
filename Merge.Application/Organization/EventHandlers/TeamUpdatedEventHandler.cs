using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Team Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class TeamUpdatedEventHandler : INotificationHandler<TeamUpdatedEvent>
{
    private readonly ILogger<TeamUpdatedEventHandler> _logger;

    public TeamUpdatedEventHandler(ILogger<TeamUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TeamUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Team updated event received. TeamId: {TeamId}, OrganizationId: {OrganizationId}, Name: {Name}",
            notification.TeamId, notification.OrganizationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Cache invalidation
        // - Notification gönderimi (team members'a)

        await Task.CompletedTask;
    }
}
