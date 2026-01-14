using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// Credit Term Activated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CreditTermActivatedEventHandler(
    ILogger<CreditTermActivatedEventHandler> logger) : INotificationHandler<CreditTermActivatedEvent>
{

    public async Task Handle(CreditTermActivatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Credit term activated event received. CreditTermId: {CreditTermId}, OrganizationId: {OrganizationId}",
            notification.CreditTermId, notification.OrganizationId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Notification to organization
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
