using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class CreditTermActivatedEventHandler(
    ILogger<CreditTermActivatedEventHandler> logger) : INotificationHandler<CreditTermActivatedEvent>
{

    public async Task Handle(CreditTermActivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Credit term activated event received. CreditTermId: {CreditTermId}, OrganizationId: {OrganizationId}",
            notification.CreditTermId, notification.OrganizationId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Notification to organization
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
