using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// Credit Term Created Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CreditTermCreatedEventHandler(
    ILogger<CreditTermCreatedEventHandler> logger) : INotificationHandler<CreditTermCreatedEvent>
{

    public async Task Handle(CreditTermCreatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation(
            "Credit term created event received. CreditTermId: {CreditTermId}, OrganizationId: {OrganizationId}, Name: {Name}, PaymentDays: {PaymentDays}, CreditLimit: {CreditLimit}",
            notification.CreditTermId, notification.OrganizationId, notification.Name, notification.PaymentDays, notification.CreditLimit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Notification to organization
        // - Analytics tracking
        // - Audit log
        // - External system sync (ERP, Accounting)

        await Task.CompletedTask;
    }
}
