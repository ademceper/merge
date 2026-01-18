using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.RemoveReviewHelpfulness;

public class RemoveReviewHelpfulnessCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<RemoveReviewHelpfulnessCommandHandler> logger) : IRequestHandler<RemoveReviewHelpfulnessCommand>
{

    public async Task Handle(RemoveReviewHelpfulnessCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Removing review helpfulness vote. UserId: {UserId}, ReviewId: {ReviewId}",
            request.UserId, request.ReviewId);

        var vote = await context.Set<ReviewHelpfulness>()
            .FirstOrDefaultAsync(rh => rh.ReviewId == request.ReviewId && rh.UserId == request.UserId, cancellationToken);

        if (vote == null) return;

        var review = await context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review != null)
        {
            if (vote.IsHelpful)
                review.UnmarkAsHelpful();
            else
                review.UnmarkAsUnhelpful();
        }

        vote.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Review helpfulness vote removed successfully. UserId: {UserId}, ReviewId: {ReviewId}",
            request.UserId, request.ReviewId);
    }
}
