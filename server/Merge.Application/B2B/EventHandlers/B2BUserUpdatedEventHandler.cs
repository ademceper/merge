using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;


public class B2BUserUpdatedEventHandler(
    ILogger<B2BUserUpdatedEventHandler> logger) : INotificationHandler<B2BUserUpdatedEvent>
{

    public async Task Handle(B2BUserUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "B2B user updated event received. B2BUserId: {B2BUserId}, UserId: {UserId}, OrganizationId: {OrganizationId}",
            notification.B2BUserId, notification.UserId, notification.OrganizationId);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation
        // - Analytics tracking
        // - Audit log
        // - External system sync (CRM, ERP)

        await Task.CompletedTask;
    }
}
