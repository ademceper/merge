using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.AddProductToComparison;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AddProductToComparisonCommand(
    Guid UserId,
    Guid ProductId
) : IRequest<ProductComparisonDto>;
