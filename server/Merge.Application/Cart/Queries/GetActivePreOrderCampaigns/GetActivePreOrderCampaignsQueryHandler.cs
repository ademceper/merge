using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Queries.GetActivePreOrderCampaigns;

public class GetActivePreOrderCampaignsQueryHandler(
    IDbContext context,
    IMapper mapper,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetActivePreOrderCampaignsQuery, PagedResult<PreOrderCampaignDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<PreOrderCampaignDto>> Handle(GetActivePreOrderCampaignsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var now = DateTime.UtcNow;
        var query = context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .Where(c => c.IsActive)
            .Where(c => c.StartDate <= now && c.EndDate >= now);

        var totalCount = await query.CountAsync(cancellationToken);

        var campaigns = await query
            .OrderBy(c => c.EndDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = mapper.Map<List<PreOrderCampaignDto>>(campaigns);

        return new PagedResult<PreOrderCampaignDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

