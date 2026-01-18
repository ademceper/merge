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
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Queries.GetAllFraudDetectionRules;

public class GetAllFraudDetectionRulesQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAllFraudDetectionRulesQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetAllFraudDetectionRulesQuery, PagedResult<FraudDetectionRuleDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<FraudDetectionRuleDto>> Handle(GetAllFraudDetectionRulesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting all fraud detection rules. RuleType: {RuleType}, IsActive: {IsActive}, Page: {Page}, PageSize: {PageSize}",
            request.RuleType, request.IsActive, request.Page, request.PageSize);

        var page = request.Page;
        var pageSize = request.PageSize;
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        IQueryable<FraudDetectionRule> query = context.Set<FraudDetectionRule>()
            .AsNoTracking();

        if (!string.IsNullOrEmpty(request.RuleType) && Enum.TryParse<FraudRuleType>(request.RuleType, true, out var rt))
        {
            query = query.Where(r => r.RuleType == rt);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rules = await query
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var ruleDtos = mapper.Map<List<FraudDetectionRuleDto>>(rules);

        logger.LogInformation("Fraud detection rules retrieved. TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}",
            totalCount, page, pageSize);

        return new PagedResult<FraudDetectionRuleDto>
        {
            Items = ruleDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
