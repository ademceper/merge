using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetReportSchedules;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetReportSchedulesQueryHandler : IRequestHandler<GetReportSchedulesQuery, PagedResult<ReportScheduleDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetReportSchedulesQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;
    private readonly PaginationSettings _paginationSettings;
    private readonly IMapper _mapper;

    public GetReportSchedulesQueryHandler(
        IDbContext context,
        ILogger<GetReportSchedulesQueryHandler> logger,
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

    public async Task<PagedResult<ReportScheduleDto>> Handle(GetReportSchedulesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching report schedules. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
            request.UserId, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (config'den)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var pageSize = request.PageSize <= 0 ? _settings.DefaultPageSize : request.PageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !s.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<ReportSchedule>()
            .AsNoTracking()
            .Where(s => s.OwnerId == request.UserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var schedules = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return new PagedResult<ReportScheduleDto>
        {
            Items = _mapper.Map<List<ReportScheduleDto>>(schedules),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

