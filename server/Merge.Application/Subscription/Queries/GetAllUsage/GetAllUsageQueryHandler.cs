using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Queries.GetAllUsage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public class GetAllUsageQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAllUsageQueryHandler> logger) : IRequestHandler<GetAllUsageQuery, PagedResult<SubscriptionUsageDto>>
{

    public async Task<PagedResult<SubscriptionUsageDto>> Handle(GetAllUsageQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > 100 ? 100 : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        // ✅ PERFORMANCE: AsNoTracking for read-only query
        IQueryable<SubscriptionUsage> query = context.Set<SubscriptionUsage>()
            .AsNoTracking()
            .Where(u => u.UserSubscriptionId == request.UserSubscriptionId &&
                       u.PeriodStart == periodStart);

        var totalCount = await query.CountAsync(cancellationToken);

        var usages = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var usageDtos = mapper.Map<IEnumerable<SubscriptionUsageDto>>(usages).ToList();

        return new PagedResult<SubscriptionUsageDto>
        {
            Items = usageDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
