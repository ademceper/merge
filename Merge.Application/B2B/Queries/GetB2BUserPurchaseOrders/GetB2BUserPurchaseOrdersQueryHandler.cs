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

namespace Merge.Application.B2B.Queries.GetB2BUserPurchaseOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetB2BUserPurchaseOrdersQueryHandler : IRequestHandler<GetB2BUserPurchaseOrdersQuery, PagedResult<PurchaseOrderDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetB2BUserPurchaseOrdersQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public GetB2BUserPurchaseOrdersQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetB2BUserPurchaseOrdersQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<PurchaseOrderDto>> Handle(GetB2BUserPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<PurchaseOrder>()
            .AsNoTracking()
            .Include(po => po.Organization)
            .Include(po => po.B2BUser!)
                .ThenInclude(b => b.User)
            .Include(po => po.Items)
                .ThenInclude(i => i.Product)
            .Where(po => po.B2BUserId == request.B2BUserId);

        if (!string.IsNullOrEmpty(request.Status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
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

