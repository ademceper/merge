using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Queries.GetAllSizeGuides;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllSizeGuidesQuery() : IRequest<IEnumerable<SizeGuideDto>>;
