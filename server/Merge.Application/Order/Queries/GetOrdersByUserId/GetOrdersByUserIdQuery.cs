using MediatR;
using Merge.Application.DTOs.Order;
using System.Collections.Generic;

namespace Merge.Application.Order.Queries.GetOrdersByUserId;

public record GetOrdersByUserIdQuery(Guid UserId, int Page = 1, int PageSize = 10) : IRequest<IEnumerable<OrderDto>>;
