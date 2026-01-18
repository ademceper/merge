using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetAbandonedCartById;

public record GetAbandonedCartByIdQuery(Guid CartId) : IRequest<AbandonedCartDto?>;

