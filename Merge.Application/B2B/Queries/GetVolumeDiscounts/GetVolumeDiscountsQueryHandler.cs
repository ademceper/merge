using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Queries.GetVolumeDiscounts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetVolumeDiscountsQueryHandler : IRequestHandler<GetVolumeDiscountsQuery, PagedResult<VolumeDiscountDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetVolumeDiscountsQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public GetVolumeDiscountsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetVolumeDiscountsQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<VolumeDiscountDto>> Handle(GetVolumeDiscountsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !vd.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<VolumeDiscount>()
            .AsNoTracking()
            .Include(vd => vd.Product)
            .Include(vd => vd.Category)
            .Include(vd => vd.Organization)
            .Where(vd => vd.IsActive);

        if (request.ProductId.HasValue)
        {
            query = query.Where(vd => vd.ProductId == request.ProductId.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(vd => vd.CategoryId == request.CategoryId.Value);
        }

        if (request.OrganizationId.HasValue)
        {
            query = query.Where(vd => vd.OrganizationId == request.OrganizationId.Value || vd.OrganizationId == null);
        }

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var discounts = await query
            .OrderBy(vd => vd.MinQuantity)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<VolumeDiscountDto>>(discounts);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<VolumeDiscountDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

