using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Blog Post Published Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record BlogPostPublishedEvent(
    Guid PostId,
    string Title,
    string Slug,
    Guid AuthorId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

