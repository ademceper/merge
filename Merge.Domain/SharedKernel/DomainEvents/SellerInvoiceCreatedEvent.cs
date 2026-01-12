using Merge.Domain.Modules.Marketplace;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// SellerInvoice Created Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record SellerInvoiceCreatedEvent(
    Guid InvoiceId,
    Guid SellerId,
    string InvoiceNumber,
    decimal NetAmount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
