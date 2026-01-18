using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.ClearSavedItems;

public record ClearSavedItemsCommand(Guid UserId) : IRequest<bool>;

