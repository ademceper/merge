using Merge.Domain.Common;

namespace Merge.Domain.Common.DomainEvents;

/// <summary>
/// Page Builder Published Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record PageBuilderPublishedEvent(
    Guid PageBuilderId,
    string Name,
    string Slug,
    Guid AuthorId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

