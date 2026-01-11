using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Commands.CreateProductFromTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateProductFromTemplateCommand(
    Guid TemplateId,
    string Name,
    string Description,
    string SKU,
    decimal Price,
    decimal? DiscountPrice,
    int StockQuantity,
    Guid? SellerId,
    Guid? StoreId,
    string? ImageUrl,
    List<string>? ImageUrls
) : IRequest<ProductDto>;
