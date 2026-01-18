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
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetReports;

public class GetReportsQueryHandler(
    IDbContext context,
    ILogger<GetReportsQueryHandler> logger,
    IOptions<AnalyticsSettings> settings,
    IOptions<PaginationSettings> paginationSettings,
    IMapper mapper) : IRequestHandler<GetReportsQuery, PagedResult<ReportDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<ReportDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching reports. UserId: {UserId}, Type: {Type}, Page: {Page}, PageSize: {PageSize}",
            request.UserId, request.Type, request.Page, request.PageSize);

        var pageSize = request.PageSize <= 0 ? paginationConfig.DefaultPageSize : request.PageSize;
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        IQueryable<Report> query = context.Set<Report>()
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

        return new PagedResult<ReportDto>
        {
            Items = mapper.Map<List<ReportDto>>(reports),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

