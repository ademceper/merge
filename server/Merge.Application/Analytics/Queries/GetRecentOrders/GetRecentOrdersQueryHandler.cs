using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetRecentOrders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetRecentOrdersQueryHandler : IRequestHandler<GetRecentOrdersQuery, IEnumerable<OrderDto>>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetRecentOrdersQueryHandler> _logger;
    private readonly AnalyticsSettings _settings;
    private readonly IMapper _mapper;

    public GetRecentOrdersQueryHandler(
        IDbContext context,
        ILogger<GetRecentOrdersQueryHandler> logger,
        IOptions<AnalyticsSettings> settings,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _settings = settings.Value;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderDto>> Handle(GetRecentOrdersQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        var count = request.Count == 10 ? _settings.TopProductsLimit : request.Count; // Recent orders için de aynı limit kullanılıyor
        
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        var orders = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }
}

