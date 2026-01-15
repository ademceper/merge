using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetPendingReturns;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetPendingReturnsQueryHandler(
    IDbContext context,
    ILogger<GetPendingReturnsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings,
    IOptions<PaginationSettings> paginationSettings,
    IMapper mapper) : IRequestHandler<GetPendingReturnsQuery, PagedResult<ReturnRequestDto>>
{

    public async Task<PagedResult<ReturnRequestDto>> Handle(GetPendingReturnsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching pending returns. Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (config'den)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var pageSize = request.PageSize <= 0 ? settings.Value.DefaultPageSize : request.PageSize;
        if (pageSize > paginationSettings.Value.MaxPageSize) pageSize = paginationSettings.Value.MaxPageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter handles it)
        var query = context.Set<ReturnRequest>()
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Order)
            .Where(r => r.Status == ReturnRequestStatus.Pending);

        var totalCount = await query.CountAsync(cancellationToken);

        var returns = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return new PagedResult<ReturnRequestDto>
        {
            Items = mapper.Map<List<ReturnRequestDto>>(returns),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

