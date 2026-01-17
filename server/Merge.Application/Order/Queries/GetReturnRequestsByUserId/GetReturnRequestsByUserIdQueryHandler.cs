using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Order;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Queries.GetReturnRequestsByUserId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetReturnRequestsByUserIdQueryHandler(IDbContext context, IMapper mapper, IOptions<OrderSettings> orderSettings) : IRequestHandler<GetReturnRequestsByUserIdQuery, PagedResult<ReturnRequestDto>>
{
    private readonly OrderSettings orderConfig = orderSettings.Value;

    public async Task<PagedResult<ReturnRequestDto>> Handle(GetReturnRequestsByUserIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU) - Configuration'dan al
        var pageSize = request.PageSize > orderConfig.MaxPageSize ? orderConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var query = context.Set<ReturnRequest>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.Order)
            .Include(r => r.User)
            .Where(r => r.UserId == request.UserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var returnRequests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Direct Map to List (no intermediate IEnumerable)
        return new PagedResult<ReturnRequestDto>
        {
            Items = mapper.Map<List<ReturnRequestDto>>(returnRequests),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
