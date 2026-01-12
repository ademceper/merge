using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// B2B User Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class B2BUserUpdatedEventHandler : INotificationHandler<B2BUserUpdatedEvent>
{
    private readonly ILogger<B2BUserUpdatedEventHandler> _logger;

    public B2BUserUpdatedEventHandler(ILogger<B2BUserUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(B2BUserUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
