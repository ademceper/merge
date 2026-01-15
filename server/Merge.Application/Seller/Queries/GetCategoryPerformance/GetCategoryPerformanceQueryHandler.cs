using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetCategoryPerformance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetCategoryPerformanceQueryHandler : IRequestHandler<GetCategoryPerformanceQuery, List<CategoryPerformanceDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetCategoryPerformanceQueryHandler> _logger;
    private readonly SellerSettings _sellerSettings;

    public GetCategoryPerformanceQueryHandler(
        IDbContext context,
        ILogger<GetCategoryPerformanceQueryHandler> logger,
        IOptions<SellerSettings> sellerSettings)
    {
        _context = context;
        _logger = logger;
        _sellerSettings = sellerSettings.Value;
    }

    public async Task<List<CategoryPerformanceDto>> Handle(GetCategoryPerformanceQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting category performance. SellerId: {SellerId}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.SellerId, request.StartDate, request.EndDate);

        // ✅ BOLUM 12.0: Magic number config'den - SellerSettings kullanımı
        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-_sellerSettings.DefaultStatsPeriodDays);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Explicit Join yaklaşımı - tek sorgu (N+1 fix)
        var categoryPerformance = await (
            from o in _context.Set<OrderEntity>().AsNoTracking()
            join oi in _context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in _context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            join c in _context.Set<Category>().AsNoTracking() on p.CategoryId equals c.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  p.SellerId == request.SellerId
            group new { oi, c } by new { CategoryId = c.Id, CategoryName = c.Name } into g
            select new CategoryPerformanceDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                TotalSales = g.Sum(x => x.oi.TotalPrice),
                OrderCount = g.Select(x => x.oi.OrderId).Distinct().Count(),
                ProductCount = g.Select(x => x.oi.ProductId).Distinct().Count()
            }
        ).OrderByDescending(c => c.TotalSales).ToListAsync(cancellationToken);

        return categoryPerformance;
    }
}
