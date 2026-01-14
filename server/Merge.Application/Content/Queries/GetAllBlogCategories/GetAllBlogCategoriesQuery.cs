using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetAllBlogCategories;

public record GetAllBlogCategoriesQuery(
    bool? IsActive = null
) : IRequest<IEnumerable<BlogCategoryDto>>;

