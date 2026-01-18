using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.ClearRecentlyViewed;

public record ClearRecentlyViewedCommand(Guid UserId) : IRequest;

