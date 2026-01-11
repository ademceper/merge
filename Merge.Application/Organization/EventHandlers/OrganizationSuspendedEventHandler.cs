using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Application.Organization.EventHandlers;

/// <summary>
/// Organization Suspended Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public class OrganizationSuspendedEventHandler : INotificationHandler<OrganizationSuspendedEvent>
{
    private readonly ILogger<OrganizationSuspendedEventHandler> _logger;

    public OrganizationSuspendedEventHandler(ILogger<OrganizationSuspendedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrganizationSuspendedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Organization suspended event received. OrganizationId: {OrganizationId}, Name: {Name}",
            notification.OrganizationId, notification.Name);

        // TODO: İleride burada şunlar yapılabilir:
        // - Suspension notification gönderimi
        // - Analytics tracking
        // - External system sync (CRM, ERP)
        // - Access revocation (API keys, etc.)

        await Task.CompletedTask;
    }
}
