using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Queries.GetFraudAlerts;

public class GetFraudAlertsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetFraudAlertsQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetFraudAlertsQuery, PagedResult<FraudAlertDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<FraudAlertDto>> Handle(GetFraudAlertsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting fraud alerts. Status: {Status}, AlertType: {AlertType}, MinRiskScore: {MinRiskScore}, Page: {Page}, PageSize: {PageSize}",
            request.Status, request.AlertType, request.MinRiskScore, request.Page, request.PageSize);

        var page = request.Page;
        var pageSize = request.PageSize;
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        IQueryable<FraudAlert> query = context.Set<FraudAlert>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.User)
            .Include(a => a.Order)
            .Include(a => a.Payment)
            .Include(a => a.ReviewedBy);

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<FraudAlertStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(a => a.Status == statusEnum);
            }
        }

        if (!string.IsNullOrEmpty(request.AlertType))
        {
            if (Enum.TryParse<FraudAlertType>(request.AlertType, true, out var alertTypeEnum))
            {
                query = query.Where(a => a.AlertType == alertTypeEnum);
            }
        }

        if (request.MinRiskScore.HasValue)
        {
            query = query.Where(a => a.RiskScore >= request.MinRiskScore.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var alerts = await query
            .OrderByDescending(a => a.RiskScore)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var alertDtos = mapper.Map<List<FraudAlertDto>>(alerts);

        logger.LogInformation("Fraud alerts retrieved. TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}",
            totalCount, page, pageSize);

        return new PagedResult<FraudAlertDto>
        {
            Items = alertDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
