using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;


public record BlogCommentDisapprovedEvent(
    Guid CommentId,
    Guid BlogPostId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
