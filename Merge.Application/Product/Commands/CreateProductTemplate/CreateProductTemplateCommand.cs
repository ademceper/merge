using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Commands.CreateProductTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateProductTemplateCommand(
    string Name,
    string Description,
    Guid CategoryId,
    string? Brand,
    string? DefaultSKUPrefix,
    decimal? DefaultPrice,
    int? DefaultStockQuantity,
    string? DefaultImageUrl,
    Dictionary<string, string>? Specifications,
    Dictionary<string, string>? Attributes,
    bool IsActive
) : IRequest<ProductTemplateDto>;
