using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Commands.SaveItem;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SaveItemCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity,
    string? Notes
) : IRequest<SavedCartItemDto>;

