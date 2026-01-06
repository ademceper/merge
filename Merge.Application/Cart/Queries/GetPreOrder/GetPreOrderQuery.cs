using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetPreOrder;

public record GetPreOrderQuery(
    Guid PreOrderId) : IRequest<PreOrderDto?>;

