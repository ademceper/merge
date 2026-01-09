using MediatR;
using Merge.Application.DTOs.Catalog;

namespace Merge.Application.Catalog.Commands.CreateCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateCategoryCommand(
    string Name,
    string Description,
    string Slug,
    string ImageUrl,
    Guid? ParentCategoryId
) : IRequest<CategoryDto>;
