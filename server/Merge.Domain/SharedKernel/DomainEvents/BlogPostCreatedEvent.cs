using Merge.Domain.SharedKernel;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Blog Post Created Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record BlogPostCreatedEvent(
    Guid PostId,
    string Title,
    string Slug,
    Guid AuthorId,
    Guid CategoryId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

