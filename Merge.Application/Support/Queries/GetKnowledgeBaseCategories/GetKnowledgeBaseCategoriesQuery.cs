using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseCategories;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetKnowledgeBaseCategoriesQuery(
    bool IncludeSubCategories = true
) : IRequest<IEnumerable<KnowledgeBaseCategoryDto>>;
