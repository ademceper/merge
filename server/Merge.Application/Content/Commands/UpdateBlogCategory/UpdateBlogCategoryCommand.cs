using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.UpdateBlogCategory;

public record UpdateBlogCategoryCommand(
    Guid Id,
    string? Name = null,
    string? Description = null,
    Guid? ParentCategoryId = null,
    string? ImageUrl = null,
    int? DisplayOrder = null,
    bool? IsActive = null
) : IRequest<bool>;

