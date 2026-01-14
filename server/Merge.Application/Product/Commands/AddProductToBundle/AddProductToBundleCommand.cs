using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.AddProductToBundle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AddProductToBundleCommand(
    Guid BundleId,
    Guid ProductId,
    int Quantity,
    int SortOrder
) : IRequest<bool>;
