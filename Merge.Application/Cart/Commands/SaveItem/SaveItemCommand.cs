using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.SaveItem;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SaveItemCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity,
    string? Notes
) : IRequest<SavedCartItemDto>;

