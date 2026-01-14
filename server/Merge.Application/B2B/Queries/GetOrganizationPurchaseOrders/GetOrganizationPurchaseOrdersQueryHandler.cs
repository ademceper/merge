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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetOrganizationPurchaseOrdersQueryHandler : IRequestHandler<GetOrganizationPurchaseOrdersQuery, PagedResult<PurchaseOrderDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrganizationPurchaseOrdersQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public GetOrganizationPurchaseOrdersQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetOrganizationPurchaseOrdersQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<PurchaseOrderDto>> Handle(GetOrganizationPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: AsSplitQuery to avoid Cartesian Explosion (multiple collection includes)
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<PurchaseOrder>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting - Multiple Include'lar için
            .Include(po => po.Organization)
            .Include(po => po.B2BUser!)
                .ThenInclude(b => b.User)
            .Include(po => po.Items)
                .ThenInclude(i => i.Product)
            .Where(po => po.OrganizationId == request.OrganizationId);

        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<PurchaseOrderStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(po => po.Status == statusEnum);
            }
        }

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var pos = await query
            .OrderByDescending(po => po.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<PurchaseOrderDto>>(pos);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<PurchaseOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

