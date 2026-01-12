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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RemoveReviewHelpfulnessCommandHandler : IRequestHandler<RemoveReviewHelpfulnessCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveReviewHelpfulnessCommandHandler> _logger;

    public RemoveReviewHelpfulnessCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RemoveReviewHelpfulnessCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RemoveReviewHelpfulnessCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Removing review helpfulness vote. UserId: {UserId}, ReviewId: {ReviewId}",
            request.UserId, request.ReviewId);

        var vote = await _context.Set<ReviewHelpfulness>()
            .FirstOrDefaultAsync(rh => rh.ReviewId == request.ReviewId && rh.UserId == request.UserId, cancellationToken);

        if (vote == null) return;

        var review = await _context.Set<ReviewEntity>()
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            if (vote.IsHelpful)
                review.UnmarkAsHelpful();
            else
                review.UnmarkAsUnhelpful();
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        vote.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Review helpfulness vote removed successfully. UserId: {UserId}, ReviewId: {ReviewId}",
            request.UserId, request.ReviewId);
    }
}
