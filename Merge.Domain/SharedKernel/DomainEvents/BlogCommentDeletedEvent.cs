using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Blog Comment Deleted Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record BlogCommentDeletedEvent(
    Guid CommentId,
    Guid BlogPostId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

