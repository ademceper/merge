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
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Queries.GetSavedItems;

public class GetSavedItemsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetSavedItemsQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetSavedItemsQuery, PagedResult<SavedCartItemDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<SavedCartItemDto>> Handle(GetSavedItemsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var query = context.Set<SavedCartItem>()
            .AsNoTracking()
            .Include(sci => sci.Product)
            .Where(sci => sci.UserId == request.UserId);

        var totalCount = await query.CountAsync(cancellationToken);

        var savedItems = await query
            .OrderByDescending(sci => sci.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = mapper.Map<List<SavedCartItemDto>>(savedItems);

        return new PagedResult<SavedCartItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

