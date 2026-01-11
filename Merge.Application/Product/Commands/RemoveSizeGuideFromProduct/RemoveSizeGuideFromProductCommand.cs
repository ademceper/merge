using MediatR;

namespace Merge.Application.Product.Commands.RemoveSizeGuideFromProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RemoveSizeGuideFromProductCommand(
    Guid ProductId
) : IRequest<bool>;
