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

namespace Merge.Application.Analytics.Queries.GetPendingReturns;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetPendingReturnsQueryHandler : IRequestHandler<GetPendingReturnsQuery, PagedResult<ReturnRequestDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetPendingReturnsQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;
    private readonly PaginationSettings _paginationSettings;
    private readonly IMapper _mapper;

    public GetPendingReturnsQueryHandler(
        IDbContext context,
        ILogger<GetPendingReturnsQueryHandler> logger,
        IOptions<AnalyticsSettings> settings,
        IOptions<PaginationSettings> paginationSettings,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
        _paginationSettings = paginationSettings.Value;
        _mapper = mapper;
    }

    public async Task<PagedResult<ReturnRequestDto>> Handle(GetPendingReturnsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching pending returns. Page: {Page}, PageSize: {PageSize}", request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (config'den)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var pageSize = request.PageSize <= 0 ? _settings.DefaultPageSize : request.PageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<ReturnRequest>()
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
            Items = _mapper.Map<List<ReturnRequestDto>>(returns),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

