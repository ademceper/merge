using MediatR;

namespace Merge.Application.Cart.Commands.ClearCart;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ClearCartCommand(Guid UserId) : IRequest<bool>;

