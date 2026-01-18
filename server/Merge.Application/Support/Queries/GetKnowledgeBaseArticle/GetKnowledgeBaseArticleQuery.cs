using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseArticle;

public record GetKnowledgeBaseArticleQuery(
    Guid ArticleId
) : IRequest<KnowledgeBaseArticleDto?>;
