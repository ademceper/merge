using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Common;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetUserComparisons;

public record GetUserComparisonsQuery(
    Guid UserId,
    bool SavedOnly = false,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductComparisonDto>>;
