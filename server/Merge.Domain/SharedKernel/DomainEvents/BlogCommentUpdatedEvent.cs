using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record BlogCommentUpdatedEvent(
    Guid CommentId,
    Guid BlogPostId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
