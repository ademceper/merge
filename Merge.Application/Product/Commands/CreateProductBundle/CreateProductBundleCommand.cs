using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Commands.CreateProductBundle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateProductBundleCommand(
    string Name,
    string Description,
    decimal BundlePrice,
    string ImageUrl,
    DateTime? StartDate,
    DateTime? EndDate,
    List<AddProductToBundleDto> Products
) : IRequest<ProductBundleDto>;
