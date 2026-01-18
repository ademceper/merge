using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class CreditTermCreatedEventHandler(
    ILogger<CreditTermCreatedEventHandler> logger) : INotificationHandler<CreditTermCreatedEvent>
{

    public async Task Handle(CreditTermCreatedEvent notification, CancellationToken cancellationToken)
    {
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
