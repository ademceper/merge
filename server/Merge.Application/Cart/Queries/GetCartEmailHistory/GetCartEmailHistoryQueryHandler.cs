using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Queries.GetCartEmailHistory;

public class GetCartEmailHistoryQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetCartEmailHistoryQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetCartEmailHistoryQuery, PagedResult<AbandonedCartEmailDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<AbandonedCartEmailDto>> Handle(GetCartEmailHistoryQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var query = context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Include(e => e.Coupon)
            .Where(e => e.CartId == request.CartId);

        var totalCount = await query.CountAsync(cancellationToken);

        var emails = await query
            .OrderByDescending(e => e.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = mapper.Map<List<AbandonedCartEmailDto>>(emails);

        return new PagedResult<AbandonedCartEmailDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

