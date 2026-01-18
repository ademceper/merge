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

public class GetVolumeDiscountsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetVolumeDiscountsQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetVolumeDiscountsQuery, PagedResult<VolumeDiscountDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<VolumeDiscountDto>> Handle(GetVolumeDiscountsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var query = context.Set<VolumeDiscount>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting - Multiple Include'lar için
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

        var totalCount = await query.CountAsync(cancellationToken);

        var discounts = await query
            .OrderBy(vd => vd.MinQuantity)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = mapper.Map<List<VolumeDiscountDto>>(discounts);

        return new PagedResult<VolumeDiscountDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

