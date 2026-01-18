using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record TaxRuleUpdatedEvent(
    Guid TaxRuleId,
    string Country,
    TaxType TaxType,
    decimal TaxRate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
