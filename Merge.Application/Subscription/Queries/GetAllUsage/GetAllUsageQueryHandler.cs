using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Domain.Entities;

namespace Merge.Application.Subscription.Queries.GetAllUsage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public class GetAllUsageQueryHandler : IRequestHandler<GetAllUsageQuery, PagedResult<SubscriptionUsageDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllUsageQueryHandler> _logger;

    public GetAllUsageQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllUsageQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<SubscriptionUsageDto>> Handle(GetAllUsageQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > 100 ? 100 : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        // ✅ PERFORMANCE: AsNoTracking for read-only query
        IQueryable<SubscriptionUsage> query = _context.Set<SubscriptionUsage>()
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
        var usageDtos = _mapper.Map<IEnumerable<SubscriptionUsageDto>>(usages).ToList();

        return new PagedResult<SubscriptionUsageDto>
        {
            Items = usageDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
