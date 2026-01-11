using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Team Activated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class TeamActivatedEventHandler : INotificationHandler<TeamActivatedEvent>
{
    private readonly ILogger<TeamActivatedEventHandler> _logger;

    public TeamActivatedEventHandler(ILogger<TeamActivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(TeamActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Team activated event received. TeamId: {TeamId}, OrganizationId: {OrganizationId}, Name: {Name}",
            notification.TeamId, notification.OrganizationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Activation notification gönderimi
        // - Analytics tracking
        // - Cache invalidation

        await Task.CompletedTask;
    }
}
