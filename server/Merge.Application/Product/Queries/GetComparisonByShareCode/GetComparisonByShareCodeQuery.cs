using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetComparisonByShareCode;

public record GetComparisonByShareCodeQuery(
    string ShareCode
) : IRequest<ProductComparisonDto?>;
