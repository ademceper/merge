using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Entities.Review;

namespace Merge.Application.Review.Commands.MarkReviewHelpfulness;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class MarkReviewHelpfulnessCommandHandler : IRequestHandler<MarkReviewHelpfulnessCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkReviewHelpfulnessCommandHandler> _logger;

    public MarkReviewHelpfulnessCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<MarkReviewHelpfulnessCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(MarkReviewHelpfulnessCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Marking review helpfulness. UserId: {UserId}, ReviewId: {ReviewId}, IsHelpful: {IsHelpful}",
            request.UserId, request.ReviewId, request.IsHelpful);

        var review = await _context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            throw new NotFoundException("Değerlendirme", request.ReviewId);
        }

        var existingVote = await _context.Set<ReviewHelpfulness>()
            .FirstOrDefaultAsync(rh => rh.ReviewId == request.ReviewId && rh.UserId == request.UserId, cancellationToken);

        if (existingVote != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
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
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var vote = ReviewHelpfulness.Create(
                request.ReviewId,
                request.UserId,
                request.IsHelpful);

            await _context.Set<ReviewHelpfulness>().AddAsync(vote, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            if (request.IsHelpful)
                review.MarkAsHelpful();
            else
                review.MarkAsUnhelpful();
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Review helpfulness marked successfully. UserId: {UserId}, ReviewId: {ReviewId}, IsHelpful: {IsHelpful}",
            request.UserId, request.ReviewId, request.IsHelpful);
    }
}
