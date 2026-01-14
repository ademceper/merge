using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.ClearRecentlyViewed;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ClearRecentlyViewedCommand(Guid UserId) : IRequest;

