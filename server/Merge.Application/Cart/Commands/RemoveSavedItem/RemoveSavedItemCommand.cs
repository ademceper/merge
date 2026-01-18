using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.RemoveSavedItem;

public record RemoveSavedItemCommand(
    Guid UserId,
    Guid ItemId
) : IRequest<bool>;

