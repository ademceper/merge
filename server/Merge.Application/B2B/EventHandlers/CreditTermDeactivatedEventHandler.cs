using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class CreditTermDeactivatedEventHandler(
    ILogger<CreditTermDeactivatedEventHandler> logger) : INotificationHandler<CreditTermDeactivatedEvent>
{

    public async Task Handle(CreditTermDeactivatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Credit term deactivated event received. CreditTermId: {CreditTermId}, OrganizationId: {OrganizationId}",
            notification.CreditTermId, notification.OrganizationId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Notification to organization
        // - Analytics tracking

        await Task.CompletedTask;
    }
}
