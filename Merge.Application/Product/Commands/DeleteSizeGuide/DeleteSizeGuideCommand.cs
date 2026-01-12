using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteSizeGuide;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteSizeGuideCommand(
    Guid Id
) : IRequest<bool>;
