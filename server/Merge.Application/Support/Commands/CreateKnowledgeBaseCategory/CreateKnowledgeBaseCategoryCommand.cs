using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Commands.CreateKnowledgeBaseCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateKnowledgeBaseCategoryCommand(
    string Name,
    string? Description = null,
    Guid? ParentCategoryId = null,
    int DisplayOrder = 0,
    bool IsActive = true,
    string? IconUrl = null
) : IRequest<KnowledgeBaseCategoryDto>;
