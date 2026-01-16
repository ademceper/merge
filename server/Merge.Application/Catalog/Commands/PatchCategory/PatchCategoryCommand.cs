using MediatR;
using Merge.Application.DTOs.Catalog;

namespace Merge.Application.Catalog.Commands.PatchCategory;

/// <summary>
/// PATCH command for partial category updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCategoryCommand(
    Guid Id,
    PatchCategoryDto PatchDto
) : IRequest<CategoryDto>;
