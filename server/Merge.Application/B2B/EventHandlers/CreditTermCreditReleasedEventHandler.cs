using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class CreditTermCreditReleasedEventHandler(
    ILogger<CreditTermCreditReleasedEventHandler> logger) : INotificationHandler<CreditTermCreditReleasedEvent>
{

    public async Task Handle(CreditTermCreditReleasedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Credit term credit released event received. CreditTermId: {CreditTermId}, OrganizationId: {OrganizationId}, Amount: {Amount}, UsedCredit: {UsedCredit}",
            notification.CreditTermId, notification.OrganizationId, notification.Amount, notification.UsedCredit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Audit log
        // - External system sync (ERP, Accounting)

        await Task.CompletedTask;
    }
}
