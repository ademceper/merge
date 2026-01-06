using MediatR;
using Merge.Application.DTOs.Catalog;

namespace Merge.Application.Catalog.Commands.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string Description,
    string Slug,
    string ImageUrl,
    Guid? ParentCategoryId
) : IRequest<CategoryDto>;
