using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Common;
using Merge.Application.DTOs.Product;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Queries.GetWishlist;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetWishlistQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetWishlistQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetWishlistQuery, PagedResult<ProductDto>>
{

    public async Task<PagedResult<ProductDto>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving wishlist (page {Page}) for user {UserId}", request.Page, request.UserId);

        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > paginationSettings.Value.MaxPageSize ? paginationSettings.Value.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (ThenInclude)
        var query = context.Set<Wishlist>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(w => w.Product)
                .ThenInclude(p => p.Category)
            .Where(w => w.UserId == request.UserId)
            .Select(w => w.Product)
            .Where(p => p.IsActive);

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);
        
        var wishlistItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} items (page {Page}) from wishlist for user {UserId}",
            wishlistItems.Count, page, request.UserId);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return new PagedResult<ProductDto>
        {
            Items = mapper.Map<List<ProductDto>>(wishlistItems),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

