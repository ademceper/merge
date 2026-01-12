using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Domain.SharedKernel.DomainEvents;

namespace Merge.Application.B2B.EventHandlers;

/// <summary>
/// Credit Term Deleted Event Handler - BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
/// </summary>
public class CreditTermDeletedEventHandler : INotificationHandler<CreditTermDeletedEvent>
{
    private readonly ILogger<CreditTermDeletedEventHandler> _logger;

    public CreditTermDeletedEventHandler(ILogger<CreditTermDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(CreditTermDeletedEvent notification, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
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
