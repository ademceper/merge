using Merge.Domain.Modules.Ordering;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record InternationalShippingUpdatedEvent(
    Guid InternationalShippingId,
    Guid OrderId,
    string UpdateType) : IDomainEvent // UpdateType: "CustomsDuty", "ImportTax", "HandlingFee", "CustomsDeclarationNumber", "EstimatedDays"
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
