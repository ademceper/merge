using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record TaxRuleDeletedEvent(
    Guid TaxRuleId,
    string Country,
    TaxType TaxType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
