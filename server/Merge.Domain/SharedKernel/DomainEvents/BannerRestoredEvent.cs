using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Banner Restored Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record BannerRestoredEvent(
    Guid Id,
    string Title,
    string Position) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
