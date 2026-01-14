using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerInvoice Cancelled Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerInvoiceCancelledEvent(
    Guid InvoiceId,
    Guid SellerId,
    string InvoiceNumber,
    string? Reason) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
