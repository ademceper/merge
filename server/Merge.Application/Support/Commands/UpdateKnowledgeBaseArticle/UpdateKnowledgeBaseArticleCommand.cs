using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Commands.UpdateKnowledgeBaseArticle;

public record UpdateKnowledgeBaseArticleCommand(
    Guid ArticleId,
    string? Title = null,
    string? Content = null,
    string? Excerpt = null,
    Guid? CategoryId = null,
    string? Status = null,
    bool? IsFeatured = null,
    int? DisplayOrder = null,
    List<string>? Tags = null
) : IRequest<KnowledgeBaseArticleDto?>;
