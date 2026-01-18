using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class CreditTermCreditUsedEventHandler(
    ILogger<CreditTermCreditUsedEventHandler> logger) : INotificationHandler<CreditTermCreditUsedEvent>
{

    public async Task Handle(CreditTermCreditUsedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Credit term credit used event received. CreditTermId: {CreditTermId}, OrganizationId: {OrganizationId}, Amount: {Amount}, UsedCredit: {UsedCredit}",
            notification.CreditTermId, notification.OrganizationId, notification.Amount, notification.UsedCredit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Credit limit warning email (eğer limit yaklaşıyorsa)
        // - Analytics tracking
        // - Audit log
        // - External system sync (ERP, Accounting)

        await Task.CompletedTask;
    }
}
