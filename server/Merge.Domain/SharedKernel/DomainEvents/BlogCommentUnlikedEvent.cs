using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record BlogCommentUnlikedEvent(
    Guid CommentId,
    Guid BlogPostId,
    int LikeCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
