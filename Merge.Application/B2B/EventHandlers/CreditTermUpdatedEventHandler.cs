using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// Credit Term Updated Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CreditTermUpdatedEventHandler : INotificationHandler<CreditTermUpdatedEvent>
{
    private readonly ILogger<CreditTermUpdatedEventHandler> _logger;

    public CreditTermUpdatedEventHandler(ILogger<CreditTermUpdatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CreditTermUpdatedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Credit term updated event received. CreditTermId: {CreditTermId}, OrganizationId: {OrganizationId}, Name: {Name}, PaymentDays: {PaymentDays}, CreditLimit: {CreditLimit}",
            notification.CreditTermId, notification.OrganizationId, notification.Name, notification.PaymentDays, notification.CreditLimit);

        // TODO: İleride burada şunlar yapılabilir:
        // - Cache invalidation
        // - Analytics tracking
        // - Audit log
        // - External system sync (ERP, Accounting)

        await Task.CompletedTask;
    }
}
