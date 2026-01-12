using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetKnowledgeBaseCategoryQuery(
    Guid CategoryId
) : IRequest<KnowledgeBaseCategoryDto?>;
