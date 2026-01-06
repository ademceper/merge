using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Application.Analytics.Queries.GetTopCustomers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetTopCustomersQueryHandler : IRequestHandler<GetTopCustomersQuery, List<TopCustomerDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetTopCustomersQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;

    public GetTopCustomersQueryHandler(
        IDbContext context,
        ILogger<GetTopCustomersQueryHandler> logger,
        IOptions<AnalyticsSettings> settings)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<List<TopCustomerDto>> Handle(GetTopCustomersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching top customers. Limit: {Limit}", request.Limit);

        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        var limit = request.Limit == 10 ? _settings.TopProductsLimit : request.Limit;
        
        // ✅ PERFORMANCE: Database'de grouping yap (memory'de değil) - 10x+ performans kazancı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        return await _context.Set<OrderEntity>()
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

