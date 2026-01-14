using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// B2B User Credit Released Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class B2BUserCreditReleasedEventHandler : INotificationHandler<B2BUserCreditReleasedEvent>
{
    private readonly ILogger<B2BUserCreditReleasedEventHandler> _logger;

    public B2BUserCreditReleasedEventHandler(ILogger<B2BUserCreditReleasedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(B2BUserCreditReleasedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "B2B user credit released event received. B2BUserId: {B2BUserId}, UserId: {UserId}, OrganizationId: {OrganizationId}, Amount: {Amount}, UsedCredit: {UsedCredit}",
            notification.B2BUserId, notification.UserId, notification.OrganizationId, notification.Amount, notification.UsedCredit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Audit log
        // - External system sync (ERP, Accounting)

        await Task.CompletedTask;
    }
}
