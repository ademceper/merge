using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Blog Comment Created Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record BlogCommentCreatedEvent(
    Guid CommentId,
    Guid BlogPostId,
    Guid? UserId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

