using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Queries.GetReviewById;

public class GetReviewByIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetReviewByIdQueryHandler> logger) : IRequestHandler<GetReviewByIdQuery, ReviewDto?>
{

    public async Task<ReviewDto?> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching review by Id: {ReviewId}", request.ReviewId);

        var review = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review is null)
        {
            logger.LogWarning("Review not found with Id: {ReviewId}", request.ReviewId);
            return null;
        }

        return mapper.Map<ReviewDto>(review);
    }
}
