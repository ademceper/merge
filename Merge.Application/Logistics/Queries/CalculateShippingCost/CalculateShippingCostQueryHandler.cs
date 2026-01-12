using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.CalculateShippingCost;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class CalculateShippingCostQueryHandler : IRequestHandler<CalculateShippingCostQuery, decimal>
{
    private readonly IDbContext _context;
    private readonly ILogger<CalculateShippingCostQueryHandler> _logger;

    public CalculateShippingCostQueryHandler(
        IDbContext context,
        ILogger<CalculateShippingCostQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<decimal> Handle(CalculateShippingCostQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calculating shipping cost. OrderId: {OrderId}, Provider: {Provider}", request.OrderId, request.ShippingProvider);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        var order = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order not found. OrderId: {OrderId}", request.OrderId);
            throw new NotFoundException("Sipariş", request.OrderId);
        }

        // Basit kargo maliyeti hesaplama
        // Gerçek uygulamada kargo firması API'sinden alınacak
        decimal baseCost = request.ShippingProvider switch
        {
            "Yurtiçi Kargo" => 50m,
            "Aras Kargo" => 45m,
            "MNG Kargo" => 40m,
            "Sürat Kargo" => 55m,
            _ => 50m
        };

        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan (BEST_PRACTICES_ANALIZI.md - BOLUM 2.1.4)
        // Not: OrderSettings inject edilmeli, şimdilik hardcoded bırakıldı (refactoring gerekli)
        if (order.SubTotal >= 500)
        {
            return 0;
        }

        // Ağırlık veya hacim bazlı hesaplama yapılabilir
        // Şimdilik sadece base cost döndürüyoruz
        return baseCost;
    }
}

