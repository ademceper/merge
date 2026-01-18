using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Catalog;

namespace Merge.Application.Catalog.Queries.GetMainCategories;

public record GetMainCategoriesQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<CategoryDto>>;

