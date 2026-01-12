using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseCategoryBySlug;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetKnowledgeBaseCategoryBySlugQuery(
    string Slug
) : IRequest<KnowledgeBaseCategoryDto?>;
