using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Queries.GetSizeGuide;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSizeGuideQuery(
    Guid Id
) : IRequest<SizeGuideDto?>;
