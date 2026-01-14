using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// BlogPost Liked Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record BlogPostLikedEvent(
    Guid BlogPostId,
    string Title,
    int LikeCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
