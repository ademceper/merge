using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetPreOrder;

public record GetPreOrderQuery(
    Guid PreOrderId) : IRequest<PreOrderDto?>;

