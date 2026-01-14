using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// FraudDetectionRule Updated Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
public record FraudDetectionRuleUpdatedEvent(
    Guid FraudDetectionRuleId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
