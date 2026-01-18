using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record SearchRecordedEvent(
    Guid SearchHistoryId,
    Guid? UserId,
    string SearchTerm,
    int ResultCount,
    string? UserAgent,
    string? IpAddress) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
