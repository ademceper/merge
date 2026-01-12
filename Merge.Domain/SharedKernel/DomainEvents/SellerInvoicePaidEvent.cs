using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerInvoice Paid Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerInvoicePaidEvent(
    Guid InvoiceId,
    Guid SellerId,
    decimal NetAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
