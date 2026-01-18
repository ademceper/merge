using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Queries.GetReviewMedia;

public record GetReviewMediaQuery(
    Guid ReviewId
) : IRequest<IEnumerable<ReviewMediaDto>>;
