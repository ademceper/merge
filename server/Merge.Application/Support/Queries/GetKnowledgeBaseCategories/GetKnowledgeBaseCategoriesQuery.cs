using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseCategories;

public record GetKnowledgeBaseCategoriesQuery(
    bool IncludeSubCategories = true
) : IRequest<IEnumerable<KnowledgeBaseCategoryDto>>;
