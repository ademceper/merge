using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Queries.GetRecentlyViewed;

public class GetRecentlyViewedQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetRecentlyViewedQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetRecentlyViewedQuery, PagedResult<ProductDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<ProductDto>> Handle(GetRecentlyViewedQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var query = context.Set<RecentlyViewedProduct>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(rvp => rvp.Product)
                .ThenInclude(p => p.Category)
            .Where(rvp => rvp.UserId == request.UserId && rvp.Product.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var recentlyViewed = await query
            .OrderByDescending(rvp => rvp.ViewedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(rvp => rvp.Product)
            .ToListAsync(cancellationToken);

        var items = mapper.Map<List<ProductDto>>(recentlyViewed);

        return new PagedResult<ProductDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

