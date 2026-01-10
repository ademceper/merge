using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;

namespace Merge.Application.Marketing.Queries.GetAllCoupons;

public record GetAllCouponsQuery(
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<PagedResult<CouponDto>>;
