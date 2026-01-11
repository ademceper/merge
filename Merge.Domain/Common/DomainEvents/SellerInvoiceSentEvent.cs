using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// SellerInvoice Sent Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerInvoiceSentEvent(
    Guid InvoiceId,
    Guid SellerId,
    string InvoiceNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
