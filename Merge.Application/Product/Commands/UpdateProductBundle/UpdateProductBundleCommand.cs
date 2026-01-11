using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Commands.UpdateProductBundle;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateProductBundleCommand(
    Guid Id,
    string Name,
    string Description,
    decimal BundlePrice,
    string ImageUrl,
    bool IsActive,
    DateTime? StartDate,
    DateTime? EndDate
) : IRequest<ProductBundleDto>;
