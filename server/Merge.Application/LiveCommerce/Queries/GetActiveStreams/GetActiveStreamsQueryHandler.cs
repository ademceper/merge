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

public class GetActiveStreamsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetActiveStreamsQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetActiveStreamsQuery, PagedResult<LiveStreamDto>>
{
    public async Task<PagedResult<LiveStreamDto>> Handle(GetActiveStreamsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting active streams. Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

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
            .Where(s => s.Status == LiveStreamStatus.Live && s.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var streams = await query
            .OrderByDescending(s => s.ActualStartTime)
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
