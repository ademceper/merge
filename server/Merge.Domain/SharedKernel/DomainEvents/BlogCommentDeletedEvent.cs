using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record BlogCommentDeletedEvent(
    Guid CommentId,
    Guid BlogPostId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

