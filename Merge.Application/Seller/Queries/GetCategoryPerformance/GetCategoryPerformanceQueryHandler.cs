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
        var categoryPerformance = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId))
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == request.SellerId))
            .GroupBy(oi => new { oi.Product.CategoryId, oi.Product.Category.Name })
            .Select(g => new CategoryPerformanceDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                TotalSales = g.Sum(oi => oi.TotalPrice),
                OrderCount = g.Count(),
                ProductCount = g.Select(oi => oi.ProductId).Distinct().Count()
            })
            .OrderByDescending(c => c.TotalSales)
            .ToListAsync(cancellationToken);

        return categoryPerformance;
    }
}
