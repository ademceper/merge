using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Product.Commands.CreateProductFromTemplate;

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
