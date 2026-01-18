using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Queries.GetProductWholesalePrices;

public record GetProductWholesalePricesQuery(
    Guid ProductId,
    Guid? OrganizationId = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<WholesalePriceDto>>;

