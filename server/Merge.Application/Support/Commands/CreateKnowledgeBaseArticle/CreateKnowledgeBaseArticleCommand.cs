using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Commands.CreateKnowledgeBaseArticle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateKnowledgeBaseArticleCommand(
    Guid AuthorId,
    string Title,
    string Content,
    string? Excerpt = null,
    Guid? CategoryId = null,
    string Status = "Draft",
    bool IsFeatured = false,
    int DisplayOrder = 0,
    List<string>? Tags = null
) : IRequest<KnowledgeBaseArticleDto>;
