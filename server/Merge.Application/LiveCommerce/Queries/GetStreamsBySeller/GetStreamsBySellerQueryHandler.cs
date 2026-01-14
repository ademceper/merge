using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.LiveCommerce.Queries.GetStreamsBySeller;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class GetStreamsBySellerQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetStreamsBySellerQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetStreamsBySellerQuery, PagedResult<LiveStreamDto>>
{
    public async Task<PagedResult<LiveStreamDto>> Handle(GetStreamsBySellerQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting streams by seller. SellerId: {SellerId}, Page: {Page}, PageSize: {PageSize}", 
            request.SellerId, request.Page, request.PageSize);

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
            .Where(s => s.SellerId == request.SellerId);

        var totalCount = await query.CountAsync(cancellationToken);

        var streams = await query
            .OrderByDescending(s => s.CreatedAt)
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

