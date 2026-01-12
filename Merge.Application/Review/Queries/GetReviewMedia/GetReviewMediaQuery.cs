using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Queries.GetReviewMedia;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetReviewMediaQuery(
    Guid ReviewId
) : IRequest<IEnumerable<ReviewMediaDto>>;
