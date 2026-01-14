using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.UpdateProductTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateProductTemplateCommand(
    Guid Id,
    string? Name,
    string? Description,
    Guid? CategoryId,
    string? Brand,
    string? DefaultSKUPrefix,
    decimal? DefaultPrice,
    int? DefaultStockQuantity,
    string? DefaultImageUrl,
    Dictionary<string, string>? Specifications,
    Dictionary<string, string>? Attributes,
    bool? IsActive
) : IRequest<bool>;
