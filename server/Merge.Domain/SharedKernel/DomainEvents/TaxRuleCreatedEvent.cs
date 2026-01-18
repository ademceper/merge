using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record TaxRuleCreatedEvent(
    Guid TaxRuleId,
    string Country,
    TaxType TaxType,
    decimal TaxRate,
    string? State,
    string? City) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
