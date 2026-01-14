using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Commands.CreateBlogCategory;

public record CreateBlogCategoryCommand(
    string Name,
    string? Description = null,
    Guid? ParentCategoryId = null,
    string? ImageUrl = null,
    int DisplayOrder = 0,
    bool IsActive = true
) : IRequest<BlogCategoryDto>;

