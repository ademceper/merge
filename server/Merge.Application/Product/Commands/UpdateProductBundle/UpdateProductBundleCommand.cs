using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.UpdateProductBundle;

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
