using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Analytics.Queries.GetSalesByCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetSalesByCategoryQueryHandler : IRequestHandler<GetSalesByCategoryQuery, List<CategorySalesDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetSalesByCategoryQueryHandler> _logger;

    public GetSalesByCategoryQueryHandler(
        IDbContext context,
        ILogger<GetSalesByCategoryQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<CategorySalesDto>> Handle(GetSalesByCategoryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching sales by category. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de değil) - 10x+ performans kazancı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !oi.IsDeleted and !oi.Order.IsDeleted checks (Global Query Filter handles it)
        return await _context.Set<OrderItem>()
            .AsNoTracking()
            .Include(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.CreatedAt >= request.StartDate && oi.Order.CreatedAt <= request.EndDate && oi.Product.Category != null)
            .GroupBy(oi => new { oi.Product.CategoryId, CategoryName = oi.Product.Category!.Name })
            // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
            .Select(g => new CategorySalesDto(
                g.Key.CategoryId,
                g.Key.CategoryName,
                g.Sum(oi => oi.TotalPrice),
                g.Select(oi => oi.OrderId).Distinct().Count(),
                g.Sum(oi => oi.Quantity)
            ))
            .OrderByDescending(c => c.Revenue)
            .ToListAsync(cancellationToken);
    }
}

