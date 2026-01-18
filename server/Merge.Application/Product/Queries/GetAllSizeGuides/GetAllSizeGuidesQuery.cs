using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetAllSizeGuides;

public record GetAllSizeGuidesQuery() : IRequest<IEnumerable<SizeGuideDto>>;
