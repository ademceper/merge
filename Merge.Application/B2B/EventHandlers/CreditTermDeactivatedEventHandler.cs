using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// Credit Term Deactivated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CreditTermDeactivatedEventHandler : INotificationHandler<CreditTermDeactivatedEvent>
{
    private readonly ILogger<CreditTermDeactivatedEventHandler> _logger;

    public CreditTermDeactivatedEventHandler(ILogger<CreditTermDeactivatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CreditTermDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Credit term deactivated event received. CreditTermId: {CreditTermId}, OrganizationId: {OrganizationId}",
            notification.CreditTermId, notification.OrganizationId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Notification to organization
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
