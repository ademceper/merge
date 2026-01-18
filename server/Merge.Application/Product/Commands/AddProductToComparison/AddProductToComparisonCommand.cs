using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.AddProductToComparison;

public record AddProductToComparisonCommand(
    Guid UserId,
    Guid ProductId
) : IRequest<ProductComparisonDto>;
