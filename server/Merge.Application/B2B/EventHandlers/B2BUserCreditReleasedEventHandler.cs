using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class B2BUserCreditReleasedEventHandler(
    ILogger<B2BUserCreditReleasedEventHandler> logger) : INotificationHandler<B2BUserCreditReleasedEvent>
{

    public async Task Handle(B2BUserCreditReleasedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "B2B user credit released event received. B2BUserId: {B2BUserId}, UserId: {UserId}, OrganizationId: {OrganizationId}, Amount: {Amount}, UsedCredit: {UsedCredit}",
            notification.B2BUserId, notification.UserId, notification.OrganizationId, notification.Amount, notification.UsedCredit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Audit log
        // - External system sync (ERP, Accounting)

        await Task.CompletedTask;
    }
}
