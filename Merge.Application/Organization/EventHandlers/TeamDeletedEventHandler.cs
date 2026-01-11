using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Team Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class TeamDeletedEventHandler : INotificationHandler<TeamDeletedEvent>
{
    private readonly ILogger<TeamDeletedEventHandler> _logger;

    public TeamDeletedEventHandler(ILogger<TeamDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TeamDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Team deleted event received. TeamId: {TeamId}, OrganizationId: {OrganizationId}, Name: {Name}",
            notification.TeamId, notification.OrganizationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Deletion notification gönderimi
        // - Analytics tracking
        // - Cache invalidation
        // - Data cleanup (if needed)

        await Task.CompletedTask;
    }
}
