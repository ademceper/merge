using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Commands.UpdateKnowledgeBaseCategory;

public record UpdateKnowledgeBaseCategoryCommand(
    Guid CategoryId,
    string? Name = null,
    string? Description = null,
    Guid? ParentCategoryId = null,
    int? DisplayOrder = null,
    bool? IsActive = null,
    string? IconUrl = null
) : IRequest<KnowledgeBaseCategoryDto?>;
