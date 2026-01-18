using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Queries.GetReviewById;

public record GetReviewByIdQuery(
    Guid ReviewId
) : IRequest<ReviewDto?>;
