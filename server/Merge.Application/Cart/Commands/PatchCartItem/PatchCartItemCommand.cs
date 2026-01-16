using MediatR;

namespace Merge.Application.Cart.Commands.PatchCartItem;

/// <summary>
/// PATCH command for partial cart item updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchCartItemCommand(
    Guid CartItemId,
    int? Quantity
) : IRequest<bool>;
