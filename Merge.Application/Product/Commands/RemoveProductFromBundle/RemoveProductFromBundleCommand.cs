using MediatR;

namespace Merge.Application.Product.Commands.RemoveProductFromBundle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RemoveProductFromBundleCommand(
    Guid BundleId,
    Guid ProductId
) : IRequest<bool>;
