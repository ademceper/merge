using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// TaxRule Deactivated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record TaxRuleDeactivatedEvent(
    Guid TaxRuleId,
    string Country,
    TaxType TaxType) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
