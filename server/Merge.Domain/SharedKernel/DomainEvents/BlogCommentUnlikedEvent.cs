using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// BlogComment Unliked Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record BlogCommentUnlikedEvent(
    Guid CommentId,
    Guid BlogPostId,
    int LikeCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
