using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SearchClickRecordedEvent(
    Guid SearchHistoryId,
    Guid ProductId,
    Guid? UserId,
    string SearchTerm) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
