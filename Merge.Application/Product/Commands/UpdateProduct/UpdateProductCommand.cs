using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Commands.UpdateProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateProductCommand(
    Guid Id,
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
    bool IsActive,
    Guid? SellerId,
    Guid? StoreId,
    Guid? PerformedBy = null // IDOR protection için
) : IRequest<ProductDto>;

