using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Content;

public record BlogCategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    IReadOnlyList<BlogCategoryDto>? SubCategories,
    int PostCount,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt
);
