using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class B2BUserCreditUsedEventHandler(
    ILogger<B2BUserCreditUsedEventHandler> logger) : INotificationHandler<B2BUserCreditUsedEvent>
{

    public async Task Handle(B2BUserCreditUsedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "B2B user credit used event received. B2BUserId: {B2BUserId}, UserId: {UserId}, OrganizationId: {OrganizationId}, Amount: {Amount}, UsedCredit: {UsedCredit}",
            notification.B2BUserId, notification.UserId, notification.OrganizationId, notification.Amount, notification.UsedCredit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Credit limit warning email (eğer limit yaklaşıyorsa)
        // - Analytics tracking
        // - Audit log
        // - External system sync (ERP, Accounting)

        await Task.CompletedTask;
    }
}
