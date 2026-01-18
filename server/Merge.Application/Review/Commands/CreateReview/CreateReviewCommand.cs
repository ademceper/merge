using MediatR;
using Merge.Application.DTOs.Review;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Review.Commands.CreateReview;

public record CreateReviewCommand(
    Guid UserId,
    Guid ProductId,
    int Rating,
    string Title,
    string Comment
) : IRequest<ReviewDto>;
