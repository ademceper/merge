using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// Blog Comment Approved Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record BlogCommentApprovedEvent(
    Guid CommentId,
    Guid BlogPostId) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

