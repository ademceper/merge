using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseArticleBySlug;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetKnowledgeBaseArticleBySlugQuery(
    string Slug
) : IRequest<KnowledgeBaseArticleDto?>;
