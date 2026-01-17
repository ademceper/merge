using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.AddToRecentlyViewed;

public record AddToRecentlyViewedCommand(
    Guid UserId,
    Guid ProductId
) : IRequest;

