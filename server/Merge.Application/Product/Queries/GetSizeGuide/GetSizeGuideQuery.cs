using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetSizeGuide;

public record GetSizeGuideQuery(
    Guid Id
) : IRequest<SizeGuideDto?>;
