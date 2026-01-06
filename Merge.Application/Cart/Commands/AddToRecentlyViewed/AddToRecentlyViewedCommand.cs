using MediatR;

namespace Merge.Application.Cart.Commands.AddToRecentlyViewed;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AddToRecentlyViewedCommand(
    Guid UserId,
    Guid ProductId
) : IRequest;

