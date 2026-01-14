using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Common;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Review.Queries.GetReviewsByUserId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public class GetReviewsByUserIdQueryHandler : IRequestHandler<GetReviewsByUserIdQuery, PagedResult<ReviewDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetReviewsByUserIdQueryHandler> _logger;
    private readonly ReviewSettings _reviewSettings;

    public GetReviewsByUserIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetReviewsByUserIdQueryHandler> logger,
        IOptions<ReviewSettings> reviewSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _reviewSettings = reviewSettings.Value;
    }

    public async Task<PagedResult<ReviewDto>> Handle(GetReviewsByUserIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > _reviewSettings.MaxPageSize
            ? _reviewSettings.MaxPageSize
            : request.PageSize;

        _logger.LogInformation(
            "Fetching reviews for user. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
            request.UserId, page, pageSize);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted check
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var query = _context.Set<ReviewEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.UserId == request.UserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Retrieved {Count} reviews for user {UserId}, page {Page}, pageSize {PageSize}",
            reviews.Count, request.UserId, page, pageSize);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var reviewDtos = _mapper.Map<IEnumerable<ReviewDto>>(reviews);

        return new PagedResult<ReviewDto>
        {
            Items = reviewDtos.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
