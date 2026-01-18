using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Queries.GetOrganizationPurchaseOrders;

public class GetOrganizationPurchaseOrdersQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetOrganizationPurchaseOrdersQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetOrganizationPurchaseOrdersQuery, PagedResult<PurchaseOrderDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<PurchaseOrderDto>> Handle(GetOrganizationPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var query = context.Set<PurchaseOrder>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting - Multiple Include'lar için
            .Include(po => po.Organization)
            .Include(po => po.B2BUser!)
                .ThenInclude(b => b.User)
            .Include(po => po.Items)
                .ThenInclude(i => i.Product)
            .Where(po => po.OrganizationId == request.OrganizationId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<PurchaseOrderStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(po => po.Status == statusEnum);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var pos = await query
            .OrderByDescending(po => po.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = mapper.Map<List<PurchaseOrderDto>>(pos);

        return new PagedResult<PurchaseOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

