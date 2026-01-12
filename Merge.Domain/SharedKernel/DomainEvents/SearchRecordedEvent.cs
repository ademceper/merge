using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Search Recorded Domain Event - BOLUM 1.5: Domain Events (ZORUNLU)
/// </summary>
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
