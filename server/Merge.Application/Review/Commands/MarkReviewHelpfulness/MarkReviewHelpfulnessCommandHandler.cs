using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Commands.MarkReviewHelpfulness;

public class MarkReviewHelpfulnessCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<MarkReviewHelpfulnessCommandHandler> logger) : IRequestHandler<MarkReviewHelpfulnessCommand>
{

    public async Task Handle(MarkReviewHelpfulnessCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Marking review helpfulness. UserId: {UserId}, ReviewId: {ReviewId}, IsHelpful: {IsHelpful}",
            request.UserId, request.ReviewId, request.IsHelpful);

        var review = await context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review is null)
        {
            throw new NotFoundException("Değerlendirme", request.ReviewId);
        }

        var existingVote = await context.Set<ReviewHelpfulness>()
            .FirstOrDefaultAsync(rh => rh.ReviewId == request.ReviewId && rh.UserId == request.UserId, cancellationToken);

        if (existingVote is not null)
        {
            if (existingVote.IsHelpful != request.IsHelpful)
            {
                // Decrement old count
                if (existingVote.IsHelpful)
                    review.UnmarkAsHelpful();
                else
                    review.UnmarkAsUnhelpful();

                // Increment new count
                if (request.IsHelpful)
                    review.MarkAsHelpful();
                else
                    review.MarkAsUnhelpful();

                existingVote.UpdateVote(request.IsHelpful);
            }
        }
        else
        {
            var vote = ReviewHelpfulness.Create(
                request.ReviewId,
                request.UserId,
                request.IsHelpful);

            await context.Set<ReviewHelpfulness>().AddAsync(vote, cancellationToken);

            if (request.IsHelpful)
                review.MarkAsHelpful();
            else
                review.MarkAsUnhelpful();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Review helpfulness marked successfully. UserId: {UserId}, ReviewId: {ReviewId}, IsHelpful: {IsHelpful}",
            request.UserId, request.ReviewId, request.IsHelpful);
    }
}
