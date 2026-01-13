using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.LiveCommerce.Queries.GetActiveStreams;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class GetActiveStreamsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetActiveStreamsQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetActiveStreamsQuery, PagedResult<LiveStreamDto>>
{
    public async Task<PagedResult<LiveStreamDto>> Handle(GetActiveStreamsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting active streams. Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Magic number YASAK - Configuration kullan
        var page = request.Page < 1 ? 1 : request.Page;
        var settings = paginationSettings.Value;
        var pageSize = request.PageSize > settings.MaxPageSize 
            ? settings.MaxPageSize 
            : request.PageSize;

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: AsSplitQuery ile Cartesian Explosion önlenir (birden fazla Include var)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var query = context.Set<LiveStream>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ EF Core 9: Query splitting - her Include ayrı sorgu
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .Where(s => s.Status == LiveStreamStatus.Live && s.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var streams = await query
            .OrderByDescending(s => s.ActualStartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (batch mapping)
        var items = mapper.Map<IEnumerable<LiveStreamDto>>(streams);

        return new PagedResult<LiveStreamDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

