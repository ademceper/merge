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

namespace Merge.Application.Cart.Queries.GetAbandonedCarts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetAbandonedCartsQueryHandler : IRequestHandler<GetAbandonedCartsQuery, PagedResult<AbandonedCartDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAbandonedCartsQueryHandler> _logger;
    private readonly PaginationSettings _paginationSettings;

    public GetAbandonedCartsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAbandonedCartsQueryHandler> logger,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<PagedResult<AbandonedCartDto>> Handle(GetAbandonedCartsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > _paginationSettings.MaxPageSize ? _paginationSettings.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        var minDate = DateTime.UtcNow.AddDays(-request.MaxDays);
        var maxDate = DateTime.UtcNow.AddHours(-request.MinHours);
        var now = DateTime.UtcNow;

        // ✅ PERFORMANCE: Database'de tüm hesaplamaları yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // Step 1: Get abandoned cart IDs (carts with items, updated in date range)
        var abandonedCartIds = await _context.Set<Merge.Domain.Entities.Cart>()
            .AsNoTracking()
            .Where(c => c.CartItems.Any() &&
                       c.UpdatedAt >= minDate &&
                       c.UpdatedAt <= maxDate)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (abandonedCartIds.Count == 0)
        {
            return new PagedResult<AbandonedCartDto>
            {
                Items = new List<AbandonedCartDto>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        // Step 2: Get user IDs for these carts
        var userIds = await _context.Set<Merge.Domain.Entities.Cart>()
            .AsNoTracking()
            .Where(c => abandonedCartIds.Contains(c.Id))
            .Select(c => c.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Step 3: Filter out carts that have been converted to orders
        var userIdsWithOrders = await _context.Set<Merge.Domain.Entities.Order>()
            .AsNoTracking()
            .Where(o => userIds.Contains(o.UserId))
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Step 4: Get final abandoned cart IDs (excluding those converted to orders)
        var finalAbandonedCartIds = await _context.Set<Merge.Domain.Entities.Cart>()
            .AsNoTracking()
            .Where(c => abandonedCartIds.Contains(c.Id) && 
                       !userIdsWithOrders.Contains(c.UserId))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (finalAbandonedCartIds.Count == 0)
        {
            return new PagedResult<AbandonedCartDto>
            {
                Items = new List<AbandonedCartDto>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = finalAbandonedCartIds.Count;

        // Step 5: Get cart data with computed properties from database
        var cartsData = await _context.Set<Merge.Domain.Entities.Cart>()
            .AsNoTracking()
            .Where(c => finalAbandonedCartIds.Contains(c.Id))
            .Select(c => new
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
            })
            .OrderByDescending(c => c.TotalValue)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Step 6: Get email stats for all carts in one query (database'de GroupBy)
        var emailStats = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => finalAbandonedCartIds.Contains(e.CartId))
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

        // Step 7: Get cart items for all carts in one query
        var cartItems = await _context.Set<CartItem>()
            .AsNoTracking()
            .Include(ci => ci.Product)
            .Where(ci => finalAbandonedCartIds.Contains(ci.CartId))
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
        var result = new List<AbandonedCartDto>();
        foreach (var c in cartsData)
        {
            var dto = new AbandonedCartDto
            {
                CartId = c.CartId,
                UserId = c.UserId,
                UserEmail = c.UserEmail,
                UserName = c.UserName,
                ItemCount = c.ItemCount,
                TotalValue = c.TotalValue,
                LastModified = c.LastModified,
                HoursSinceAbandonment = c.HoursSinceAbandonment,
                Items = cartItemsDict.ContainsKey(c.CartId)
                    ? _mapper.Map<IEnumerable<CartItemDto>>(cartItemsDict[c.CartId]).ToList()
                    : new List<CartItemDto>(),
                HasReceivedEmail = emailStatsDict.ContainsKey(c.CartId) && emailStatsDict[c.CartId].HasReceivedEmail,
                EmailsSentCount = emailStatsDict.ContainsKey(c.CartId) ? emailStatsDict[c.CartId].EmailsSentCount : 0,
                LastEmailSent = emailStatsDict.ContainsKey(c.CartId) ? emailStatsDict[c.CartId].LastEmailSent : null
            };
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

