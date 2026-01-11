using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Invoice Paid Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record InvoicePaidEvent(
    Guid InvoiceId,
    Guid OrderId,
    string InvoiceNumber,
    decimal TotalAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
