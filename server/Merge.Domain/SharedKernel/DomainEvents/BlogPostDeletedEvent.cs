using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Blog Post Deleted Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record BlogPostDeletedEvent(
    Guid PostId,
    string Title) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

