using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetCustomerAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetCustomerAnalyticsQueryHandler : IRequestHandler<GetCustomerAnalyticsQuery, CustomerAnalyticsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetCustomerAnalyticsQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;
    private readonly IMapper _mapper;

    public GetCustomerAnalyticsQueryHandler(
        IDbContext context,
        ILogger<GetCustomerAnalyticsQueryHandler> logger,
        IOptions<AnalyticsSettings> settings,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
        _mapper = mapper;
    }

    public async Task<CustomerAnalyticsDto> Handle(GetCustomerAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching customer analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ Identity framework'ün Role ve UserRole entity'leri IDbContext üzerinden erişiliyor
        var customerRole = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == "Customer", cancellationToken);
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var customerUserIds = customerRole != null
            ? await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.RoleId == customerRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync(cancellationToken)
            : new List<Guid>(0); // Pre-allocate with known capacity (0)
        
        // ✅ PERFORMANCE: Database'de filtreleme yap (memory'de değil)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !u.IsDeleted and !o.IsDeleted checks (Global Query Filter handles it)
        // ✅ PERFORMANCE: List.Count > 0 kullan (Any() YASAK - .cursorrules)
        var totalCustomers = customerUserIds.Count > 0
            ? await _context.Users
                .AsNoTracking()
                .Where(u => customerUserIds.Contains(u.Id))
                .CountAsync(cancellationToken)
            : 0;

        var newCustomers = customerUserIds.Count > 0
            ? await _context.Users
                .AsNoTracking()
                .Where(u => customerUserIds.Contains(u.Id) && u.CreatedAt >= request.StartDate && u.CreatedAt <= request.EndDate)
                .CountAsync(cancellationToken)
            : 0;

        var activeCustomers = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate)
            .Select(o => o.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var topCustomers = await GetTopCustomersAsync(_settings.MaxQueryLimit, cancellationToken);
        var customerSegments = await GetCustomerSegmentsAsync(cancellationToken);
        
        return new CustomerAnalyticsDto(
            request.StartDate,
            request.EndDate,
            totalCustomers,
            newCustomers,
            activeCustomers,
            0, // ReturningCustomers - şimdilik 0
            0, // AverageLifetimeValue - şimdilik 0
            0, // AveragePurchaseFrequency - şimdilik 0
            customerSegments,
            topCustomers,
            new List<TimeSeriesDataPoint>() // CustomerAcquisition - şimdilik boş
        );
    }

    private async Task<List<TopCustomerDto>> GetTopCustomersAsync(int limit, CancellationToken cancellationToken)
    {
        return await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.User)
            .GroupBy(o => new { o.UserId, o.User.FirstName, o.User.LastName, o.User.Email })
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

    private Task<List<CustomerSegmentDto>> GetCustomerSegmentsAsync(CancellationToken cancellationToken)
    {
        // Simplified segmentation - can be enhanced
        // ✅ ARCHITECTURE: .cursorrules'a göre manuel mapping YASAK, AutoMapper kullanıyoruz
        var segmentsData = new[]
        {
            new { Segment = "VIP", CustomerCount = 0, TotalRevenue = 0m, AverageOrderValue = 0m },
            new { Segment = "Active", CustomerCount = 0, TotalRevenue = 0m, AverageOrderValue = 0m },
            new { Segment = "New", CustomerCount = 0, TotalRevenue = 0m, AverageOrderValue = 0m }
        };

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var segments = _mapper.Map<List<CustomerSegmentDto>>(segmentsData);
        return Task.FromResult(segments);
    }
}

