using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetCartByUserId;

public record GetCartByUserIdQuery(Guid UserId) : IRequest<CartDto>;

