using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PageBuilder Viewed Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record PageBuilderViewedEvent(
    Guid PageBuilderId,
    string Name,
    int ViewCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
