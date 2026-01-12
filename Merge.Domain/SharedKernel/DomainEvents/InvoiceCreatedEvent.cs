using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Invoice Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record InvoiceCreatedEvent(
    Guid InvoiceId,
    Guid OrderId,
    string InvoiceNumber,
    decimal TotalAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
