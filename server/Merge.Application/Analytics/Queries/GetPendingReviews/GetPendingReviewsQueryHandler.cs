using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.Review;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetPendingReviews;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetPendingReviewsQueryHandler(
    IDbContext context,
    ILogger<GetPendingReviewsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings,
    IOptions<PaginationSettings> paginationSettings,
    IMapper mapper) : IRequestHandler<GetPendingReviewsQuery, PagedResult<ReviewDto>>
{

    public async Task<PagedResult<ReviewDto>> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching pending reviews. Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (config'den)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var pageSize = request.PageSize <= 0 ? settings.Value.DefaultPageSize : request.PageSize;
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter handles it)
        var query = context.Set<ReviewEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => !r.IsApproved);

        var totalCount = await query.CountAsync(cancellationToken);

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return new PagedResult<ReviewDto>
        {
            Items = mapper.Map<List<ReviewDto>>(reviews),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

