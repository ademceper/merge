using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.CreateProductBundle;

public record CreateProductBundleCommand(
    string Name,
    string Description,
    decimal BundlePrice,
    string ImageUrl,
    DateTime? StartDate,
    DateTime? EndDate,
    List<AddProductToBundleDto> Products
) : IRequest<ProductBundleDto>;
