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
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using CartEntity = Merge.Domain.Modules.Ordering.Cart;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Cart.Queries.GetAbandonedCarts;

public class GetAbandonedCartsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAbandonedCartsQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetAbandonedCartsQuery, PagedResult<AbandonedCartDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<AbandonedCartDto>> Handle(GetAbandonedCartsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var minDate = DateTime.UtcNow.AddDays(-request.MaxDays);
        var maxDate = DateTime.UtcNow.AddHours(-request.MinHours);
        var now = DateTime.UtcNow;

        // Step 1-4: Get final abandoned cart IDs using subqueries (no materialization)
        var abandonedCartsQuery = context.Set<CartEntity>()
            .AsNoTracking()
            .Where(c => c.CartItems.Any() &&
                       c.UpdatedAt >= minDate &&
                       c.UpdatedAt <= maxDate);

        // Get user IDs for abandoned carts (materialize for Contains)
        var userIds = await (from c in abandonedCartsQuery
                          select c.UserId)
                          .Distinct()
                          .ToListAsync(cancellationToken);

        // Filter out carts that have been converted to orders
        var userIdsWithOrders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => userIds.Contains(o.UserId))
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Get final abandoned cart IDs (excluding those converted to orders)
        var finalAbandonedCartIdsQuery = from c in abandonedCartsQuery
                                         where !userIdsWithOrders.Contains(c.UserId)
                                         select c.Id;

        // Check if any abandoned carts exist
        var hasAbandonedCarts = await finalAbandonedCartIdsQuery.AnyAsync(cancellationToken);
        if (!hasAbandonedCarts)
        {
            return new PagedResult<AbandonedCartDto>
            {
                Items = [],
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        var totalCount = await finalAbandonedCartIdsQuery.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return new PagedResult<AbandonedCartDto>
            {
                Items = [],
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        // Step 5: Get cart data with computed properties from database (subquery ile pagination)
        var cartsDataQuery = (
            from c in context.Set<CartEntity>().AsNoTracking()
            where finalAbandonedCartIdsQuery.Contains(c.Id)
            select new
            {
                CartId = c.Id,
                UserId = c.UserId,
                UserEmail = c.User != null ? c.User.Email : "",
                UserName = c.User != null ? (c.User.FirstName + " " + c.User.LastName) : "",
                LastModified = c.UpdatedAt ?? c.CreatedAt,
                HoursSinceAbandonment = c.UpdatedAt.HasValue 
                    ? (int)((now - c.UpdatedAt.Value).TotalHours)
                    : (int)((now - c.CreatedAt).TotalHours),
                ItemCount = c.CartItems.Count,
                TotalValue = c.CartItems.Sum(ci => ci.Price * ci.Quantity)
            }
        )
        .OrderByDescending(c => c.TotalValue)
        .Skip((page - 1) * pageSize)
        .Take(pageSize);

        var cartsData = await cartsDataQuery.ToListAsync(cancellationToken);

        // Step 6: Get email stats for all carts in one query (database'de GroupBy) - subquery ile
        var paginatedCartIds = await (from c in cartsDataQuery select c.CartId).ToListAsync(cancellationToken);
        var emailStats = await context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => paginatedCartIds.Contains(e.CartId))
            .GroupBy(e => e.CartId)
            .Select(g => new
            {
                CartId = g.Key,
                EmailsSentCount = g.Count(),
                HasReceivedEmail = g.Any(),
                LastEmailSent = g.OrderByDescending(e => e.SentAt).Select(e => (DateTime?)e.SentAt).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var emailStatsDict = new Dictionary<Guid, (int EmailsSentCount, bool HasReceivedEmail, DateTime? LastEmailSent)>(emailStats.Count);
        foreach (var stat in emailStats)
        {
            emailStatsDict[stat.CartId] = (stat.EmailsSentCount, stat.HasReceivedEmail, stat.LastEmailSent);
        }

        // Step 7: Get cart items for all carts in one query - subquery ile
        var cartItems = await context.Set<CartItem>()
            .AsNoTracking()
            .Include(ci => ci.Product)
            .Where(ci => paginatedCartIds.Contains(ci.CartId))
            .ToListAsync(cancellationToken);

        Dictionary<Guid, List<CartItem>> cartItemsDict = [];
        foreach (var item in cartItems)
        {
            if (!cartItemsDict.ContainsKey(item.CartId))
            {
                cartItemsDict[item.CartId] = [];
            }
            cartItemsDict[item.CartId].Add(item);
        }
        
        List<AbandonedCartDto> result = [];
        foreach (var c in cartsData)
        {
            var items = cartItemsDict.ContainsKey(c.CartId)
                ? mapper.Map<IEnumerable<CartItemDto>>(cartItemsDict[c.CartId]).ToList().AsReadOnly()
                : new List<CartItemDto>().AsReadOnly();
            
            var dto = new AbandonedCartDto(
                c.CartId,
                c.UserId,
                c.UserEmail ?? string.Empty,
                c.UserName ?? string.Empty,
                c.ItemCount,
                c.TotalValue,
                c.LastModified,
                c.HoursSinceAbandonment,
                items,
                emailStatsDict.ContainsKey(c.CartId) && emailStatsDict[c.CartId].HasReceivedEmail,
                emailStatsDict.ContainsKey(c.CartId) ? emailStatsDict[c.CartId].EmailsSentCount : 0,
                emailStatsDict.ContainsKey(c.CartId) ? emailStatsDict[c.CartId].LastEmailSent : null
            );
            result.Add(dto);
        }

        return new PagedResult<AbandonedCartDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

