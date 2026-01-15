using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetTopCustomers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetTopCustomersQueryHandler(
    IDbContext context,
    ILogger<GetTopCustomersQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetTopCustomersQuery, List<TopCustomerDto>>
{

    public async Task<List<TopCustomerDto>> Handle(GetTopCustomersQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching top customers. Limit: {Limit}", request.Limit);

        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        var limit = request.Limit == 10 ? settings.Value.TopProductsLimit : request.Limit;
        
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de değil) - 10x+ performans kazancı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        return await context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.User)
            .GroupBy(o => new { o.UserId, o.User.FirstName, o.User.LastName, o.User.Email })
            // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
            .Select(g => new TopCustomerDto(
                g.Key.UserId,
                $"{g.Key.FirstName} {g.Key.LastName}",
                g.Key.Email ?? string.Empty,
                g.Count(),
                g.Sum(o => o.TotalAmount),
                g.Max(o => o.CreatedAt)
            ))
            .OrderByDescending(c => c.TotalSpent)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}

