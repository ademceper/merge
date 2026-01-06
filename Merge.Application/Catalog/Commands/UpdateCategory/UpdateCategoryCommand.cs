using MediatR;
using Merge.Application.DTOs.Catalog;

namespace Merge.Application.Catalog.Commands.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Description,
    string Slug,
    string ImageUrl,
    Guid? ParentCategoryId
) : IRequest<CategoryDto>;
