using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class CreditTermDeletedEventHandler(
    ILogger<CreditTermDeletedEventHandler> logger) : INotificationHandler<CreditTermDeletedEvent>
{

    public async Task Handle(CreditTermDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Credit term deleted event received. CreditTermId: {CreditTermId}, OrganizationId: {OrganizationId}",
            notification.CreditTermId, notification.OrganizationId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation
        // - Analytics tracking
        // - Audit log
        // - External system sync (ERP, Accounting)

        await Task.CompletedTask;
    }
}
