using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Invoice Sent Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record InvoiceSentEvent(
    Guid InvoiceId,
    Guid OrderId,
    string InvoiceNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
