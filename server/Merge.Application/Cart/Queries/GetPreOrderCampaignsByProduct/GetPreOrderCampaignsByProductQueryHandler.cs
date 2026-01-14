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

namespace Merge.Application.Cart.Queries.GetPreOrderCampaignsByProduct;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetPreOrderCampaignsByProductQueryHandler : IRequestHandler<GetPreOrderCampaignsByProductQuery, PagedResult<PreOrderCampaignDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly PaginationSettings _paginationSettings;

    public GetPreOrderCampaignsByProductQueryHandler(
        IDbContext context,
        IMapper mapper,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<PreOrderCampaignDto>> Handle(GetPreOrderCampaignsByProductQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var query = _context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .Where(c => c.ProductId == request.ProductId);

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var campaigns = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<PreOrderCampaignDto>>(campaigns);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<PreOrderCampaignDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

