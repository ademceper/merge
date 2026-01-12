using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// Credit Term Credit Released Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CreditTermCreditReleasedEventHandler : INotificationHandler<CreditTermCreditReleasedEvent>
{
    private readonly ILogger<CreditTermCreditReleasedEventHandler> _logger;

    public CreditTermCreditReleasedEventHandler(ILogger<CreditTermCreditReleasedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CreditTermCreditReleasedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Credit term credit released event received. CreditTermId: {CreditTermId}, OrganizationId: {OrganizationId}, Amount: {Amount}, UsedCredit: {UsedCredit}",
            notification.CreditTermId, notification.OrganizationId, notification.Amount, notification.UsedCredit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Analytics tracking
        // - Audit log
        // - External system sync (ERP, Accounting)

        await Task.CompletedTask;
    }
}
