using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.DeleteProductBundle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteProductBundleCommand(
    Guid Id
) : IRequest<bool>;
