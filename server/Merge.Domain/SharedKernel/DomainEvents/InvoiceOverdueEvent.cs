using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Invoice Overdue Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record InvoiceOverdueEvent(
    Guid InvoiceId,
    Guid OrderId,
    string InvoiceNumber,
    DateTime DueDate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
