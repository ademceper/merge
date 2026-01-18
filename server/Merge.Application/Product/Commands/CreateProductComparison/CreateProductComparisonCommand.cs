using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.CreateProductComparison;

public record CreateProductComparisonCommand(
    Guid UserId,
    string? Name,
    List<Guid> ProductIds
) : IRequest<ProductComparisonDto>;
