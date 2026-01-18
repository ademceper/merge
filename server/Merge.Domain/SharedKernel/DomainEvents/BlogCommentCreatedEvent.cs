using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record BlogCommentCreatedEvent(
    Guid CommentId,
    Guid BlogPostId,
    Guid? UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

