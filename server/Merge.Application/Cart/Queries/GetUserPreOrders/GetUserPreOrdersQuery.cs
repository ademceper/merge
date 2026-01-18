using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetUserPreOrders;

public record GetUserPreOrdersQuery(
    Guid UserId,
    int Page,
    int PageSize) : IRequest<PagedResult<PreOrderDto>>;

