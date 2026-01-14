using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// KnowledgeBaseArticle Viewed Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record KnowledgeBaseArticleViewedEvent(
    Guid ArticleId,
    string Title,
    int ViewCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
