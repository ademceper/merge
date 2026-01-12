using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Product.Commands.CreateProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateProductCommand(
    string Name,
    string Description,
    string SKU,
    decimal Price,
    decimal? DiscountPrice,
    int StockQuantity,
    string Brand,
    string ImageUrl,
    List<string> ImageUrls,
    Guid CategoryId,
    Guid? SellerId,
    Guid? StoreId
) : IRequest<ProductDto>;

