using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.ClearCart;

public record ClearCartCommand(Guid UserId) : IRequest<bool>;

