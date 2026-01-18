using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetCartByCartItemId;

public record GetCartByCartItemIdQuery(Guid CartItemId) : IRequest<CartDto?>;

