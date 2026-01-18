using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Queries.GetAllPayouts;

public record GetAllPayoutsQuery(
    PayoutStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<CommissionPayoutDto>>;
