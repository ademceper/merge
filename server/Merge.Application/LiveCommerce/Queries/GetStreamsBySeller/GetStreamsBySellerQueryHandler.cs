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

        var page = request.Page < 1 ? 1 : request.Page;
        var settings = paginationSettings.Value;
        var pageSize = request.PageSize > settings.MaxPageSize 
            ? settings.MaxPageSize 
            : request.PageSize;

        var query = context.Set<LiveStream>()
            .AsNoTracking()
            .AsSplitQuery()
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
