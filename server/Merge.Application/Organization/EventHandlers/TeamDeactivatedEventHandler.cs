using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Team Deactivated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class TeamDeactivatedEventHandler : INotificationHandler<TeamDeactivatedEvent>
{
    private readonly ILogger<TeamDeactivatedEventHandler> _logger;

    public TeamDeactivatedEventHandler(ILogger<TeamDeactivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TeamDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Team deactivated event received. TeamId: {TeamId}, OrganizationId: {OrganizationId}, Name: {Name}",
            notification.TeamId, notification.OrganizationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Deactivation notification gönderimi
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
