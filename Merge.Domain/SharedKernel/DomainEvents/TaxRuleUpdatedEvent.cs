using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// TaxRule Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TaxRuleUpdatedEvent(
    Guid TaxRuleId,
    string Country,
    TaxType TaxType,
    decimal TaxRate) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
