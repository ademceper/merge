using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using AutoMapper;

namespace Merge.Application.Analytics.Queries.GetReports;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, PagedResult<ReportDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetReportsQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;
    private readonly PaginationSettings _paginationSettings;
    private readonly IMapper _mapper;

    public GetReportsQueryHandler(
        IDbContext context,
        ILogger<GetReportsQueryHandler> logger,
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

    public async Task<PagedResult<ReportDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching reports. UserId: {UserId}, Type: {Type}, Page: {Page}, PageSize: {PageSize}",
            request.UserId, request.Type, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (config'den)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var pageSize = request.PageSize <= 0 ? _settings.DefaultPageSize : request.PageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        IQueryable<Report> query = _context.Set<Report>()
            .AsNoTracking()
            .Include(r => r.GeneratedByUser);

        if (request.UserId.HasValue)
        {
            query = query.Where(r => r.GeneratedBy == request.UserId.Value);
        }

        if (!string.IsNullOrEmpty(request.Type) && Enum.TryParse<ReportType>(request.Type, true, out var reportType))
        {
            query = query.Where(r => r.Type == reportType);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return new PagedResult<ReportDto>
        {
            Items = _mapper.Map<List<ReportDto>>(reports),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

