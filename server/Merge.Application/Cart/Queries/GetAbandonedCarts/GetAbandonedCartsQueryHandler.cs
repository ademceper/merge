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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAbandonedCartsQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetAbandonedCartsQueryHandler> logger,
    IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetAbandonedCartsQuery, PagedResult<AbandonedCartDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<PagedResult<AbandonedCartDto>> Handle(GetAbandonedCartsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var minDate = DateTime.UtcNow.AddDays(-request.MaxDays);
        var maxDate = DateTime.UtcNow.AddHours(-request.MinHours);
        var now = DateTime.UtcNow;

        // ✅ PERFORMANCE: Database'de tüm hesaplamaları yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: Subquery yaklaşımı - memory'de hiçbir şey tutma (ISSUE #3.1 fix)
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
                Items = new List<AbandonedCartDto>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        // ✅ PERFORMANCE: TotalCount için subquery kullan (memory'de materialize etme)
        var totalCount = await finalAbandonedCartIdsQuery.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return new PagedResult<AbandonedCartDto>
            {
                Items = new List<AbandonedCartDto>(),
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
                // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching (ifade ağacında != null kullan)
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

        // ✅ PERFORMANCE: Dictionary oluşturma minimal bir işlem (O(n) lookup için gerekli)
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
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

        // ✅ PERFORMANCE: Dictionary oluşturma minimal bir işlem (O(1) lookup için gerekli)
        var cartItemsDict = new Dictionary<Guid, List<CartItem>>();
        foreach (var item in cartItems)
        {
            if (!cartItemsDict.ContainsKey(item.CartId))
            {
                cartItemsDict[item.CartId] = new List<CartItem>();
            }
            cartItemsDict[item.CartId].Add(item);
        }

        // Step 8: Build DTOs (minimal memory operations - only property assignment)
        // ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı) - Positional constructor kullanımı
        var result = new List<AbandonedCartDto>();
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

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<AbandonedCartDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}

