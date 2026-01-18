using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class CreditTermUpdatedEventHandler(
    ILogger<CreditTermUpdatedEventHandler> logger) : INotificationHandler<CreditTermUpdatedEvent>
{

    public async Task Handle(CreditTermUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Credit term updated event received. CreditTermId: {CreditTermId}, OrganizationId: {OrganizationId}, Name: {Name}, PaymentDays: {PaymentDays}, CreditLimit: {CreditLimit}",
            notification.CreditTermId, notification.OrganizationId, notification.Name, notification.PaymentDays, notification.CreditLimit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation
        // - Analytics tracking
        // - Audit log
        // - External system sync (ERP, Accounting)

        await Task.CompletedTask;
    }
}
