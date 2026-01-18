using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseCategory;

public record GetKnowledgeBaseCategoryQuery(
    Guid CategoryId
) : IRequest<KnowledgeBaseCategoryDto?>;
