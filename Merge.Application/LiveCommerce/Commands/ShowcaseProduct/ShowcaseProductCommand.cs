using MediatR;

namespace Merge.Application.LiveCommerce.Commands.ShowcaseProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ShowcaseProductCommand(
    Guid StreamId,
    Guid ProductId) : IRequest<Unit>;

