using MediatR;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Review.Commands.DeleteReview;

public record DeleteReviewCommand(
    Guid ReviewId,
    Guid UserId // IDOR protection
) : IRequest<bool>;
