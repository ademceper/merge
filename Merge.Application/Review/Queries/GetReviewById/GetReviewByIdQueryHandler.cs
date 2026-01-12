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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetReviewByIdQueryHandler : IRequestHandler<GetReviewByIdQuery, ReviewDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetReviewByIdQueryHandler> _logger;

    public GetReviewByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetReviewByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReviewDto?> Handle(GetReviewByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching review by Id: {ReviewId}", request.ReviewId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var review = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == request.ReviewId, cancellationToken);

        if (review == null)
        {
            _logger.LogWarning("Review not found with Id: {ReviewId}", request.ReviewId);
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<ReviewDto>(review);
    }
}
