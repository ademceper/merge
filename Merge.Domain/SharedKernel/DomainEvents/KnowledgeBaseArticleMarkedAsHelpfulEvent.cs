using Merge.Domain.SharedKernel;

namespace Merge.Domain.SharedKernel.DomainEvents;

/// <summary>
/// KnowledgeBaseArticle Marked As Helpful Domain Event - BOLUM 1.5: Domain Events (ÖNERİLİR)
/// </summary>
public record KnowledgeBaseArticleMarkedAsHelpfulEvent(
    Guid ArticleId,
    string Title,
    int HelpfulCount) : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
