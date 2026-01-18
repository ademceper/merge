using Merge.Domain.Modules.Payment;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record FraudDetectionRuleUpdatedEvent(
    Guid FraudDetectionRuleId,
    string Name) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
