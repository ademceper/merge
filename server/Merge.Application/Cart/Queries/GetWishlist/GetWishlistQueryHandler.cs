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

public class GetWishlistQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetWishlistQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetWishlistQuery, PagedResult<ProductDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<ProductDto>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving wishlist (page {Page}) for user {UserId}", request.Page, request.UserId);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var query = context.Set<Wishlist>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(w => w.Product)
                .ThenInclude(p => p.Category)
            .Where(w => w.UserId == request.UserId)
            .Select(w => w.Product)
            .Where(p => p.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var wishlistItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} items (page {Page}) from wishlist for user {UserId}",
            wishlistItems.Count, page, request.UserId);

        return new PagedResult<ProductDto>
        {
            Items = mapper.Map<List<ProductDto>>(wishlistItems),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

