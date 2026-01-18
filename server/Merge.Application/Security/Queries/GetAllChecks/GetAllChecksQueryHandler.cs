using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Queries.GetAllChecks;

public class GetAllChecksQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAllChecksQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetAllChecksQuery, PagedResult<PaymentFraudPreventionDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;


    public async Task<PagedResult<PaymentFraudPreventionDto>> Handle(GetAllChecksQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Payment fraud check'ler sorgulanıyor. Status: {Status}, IsBlocked: {IsBlocked}, Page: {Page}, PageSize: {PageSize}",
            request.Status ?? "All", request.IsBlocked?.ToString() ?? "All", request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        IQueryable<PaymentFraudPrevention> query = context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment);

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<VerificationStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(c => c.Status == statusEnum);
            }
        }

        if (request.IsBlocked.HasValue)
        {
            query = query.Where(c => c.IsBlocked == request.IsBlocked.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var checks = await query
            .OrderByDescending(c => c.RiskScore)
            .ThenByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var checkDtos = mapper.Map<IEnumerable<PaymentFraudPreventionDto>>(checks).ToList();

        logger.LogInformation("Payment fraud check'ler bulundu. TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}, ReturnedCount: {ReturnedCount}",
            totalCount, page, pageSize, checkDtos.Count);

        return new PagedResult<PaymentFraudPreventionDto>
        {
            Items = checkDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
