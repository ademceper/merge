using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetAbandonedCarts;

public record GetAbandonedCartsQuery(
    int MinHours = 1,
    int MaxDays = 30,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<AbandonedCartDto>>;

