using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Queries.GetActiveStreams;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetActiveStreamsQueryHandler : IRequestHandler<GetActiveStreamsQuery, PagedResult<LiveStreamDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetActiveStreamsQueryHandler> _logger;

    public GetActiveStreamsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetActiveStreamsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<LiveStreamDto>> Handle(GetActiveStreamsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting active streams. Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize > 100 ? 100 : request.PageSize; // Max limit

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var query = _context.Set<LiveStream>()
            .AsNoTracking()
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
        var items = _mapper.Map<IEnumerable<LiveStreamDto>>(streams);

        return new PagedResult<LiveStreamDto>
        {
            Items = items.ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

