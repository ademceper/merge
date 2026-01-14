using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// PageBuilder Restored Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record PageBuilderRestoredEvent(
    Guid Id,
    string Name,
    string Slug) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
