using MediatR;
using Merge.Application.DTOs.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Catalog.Commands.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Description,
    string Slug,
    string ImageUrl,
    Guid? ParentCategoryId
) : IRequest<CategoryDto>;
