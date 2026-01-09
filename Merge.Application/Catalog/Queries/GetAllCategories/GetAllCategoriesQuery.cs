using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Catalog;

namespace Merge.Application.Catalog.Queries.GetAllCategories;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllCategoriesQuery(
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<CategoryDto>>;

