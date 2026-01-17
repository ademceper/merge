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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAllFraudDetectionRulesQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAllFraudDetectionRulesQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetAllFraudDetectionRulesQuery, PagedResult<FraudDetectionRuleDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<FraudDetectionRuleDto>> Handle(GetAllFraudDetectionRulesQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Getting all fraud detection rules. RuleType: {RuleType}, IsActive: {IsActive}, Page: {Page}, PageSize: {PageSize}",
            request.RuleType, request.IsActive, request.Page, request.PageSize);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU)
        // ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
        var page = request.Page;
        var pageSize = request.PageSize;
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        IQueryable<FraudDetectionRule> query = context.Set<FraudDetectionRule>()
            .AsNoTracking();

        // ✅ BOLUM 1.2: Enum kullanımı (string RuleType YASAK)
        if (!string.IsNullOrEmpty(request.RuleType) && Enum.TryParse<FraudRuleType>(request.RuleType, true, out var rt))
        {
            query = query.Where(r => r.RuleType == rt);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == request.IsActive.Value);
        }

        // ✅ PERFORMANCE: Database'de Count ve pagination yap (memory'de işlem YASAK)
        var totalCount = await query.CountAsync(cancellationToken);
        var rules = await query
            .OrderByDescending(r => r.Priority)
            .ThenBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
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
