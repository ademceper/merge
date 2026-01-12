using MediatR;
using Merge.Application.DTOs.Order;

namespace Merge.Application.Order.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;
