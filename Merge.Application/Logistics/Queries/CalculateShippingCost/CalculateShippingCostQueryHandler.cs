using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
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
    private readonly ShippingSettings _shippingSettings;
    private readonly OrderSettings _orderSettings;

    public CalculateShippingCostQueryHandler(
        IDbContext context,
        ILogger<CalculateShippingCostQueryHandler> logger,
        IOptions<ShippingSettings> shippingSettings,
        IOptions<OrderSettings> orderSettings)
    {
        _context = context;
        _logger = logger;
        _shippingSettings = shippingSettings.Value;
        _orderSettings = orderSettings.Value;
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

        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan (BEST_PRACTICES_ANALIZI.md - BOLUM 2.1.4)
        // Kargo sağlayıcısına göre maliyet hesaplama
        decimal baseCost = _shippingSettings.DefaultShippingCost;
        
        // Provider code'a göre maliyet bul (örn: "YURTICI", "ARAS", "MNG", "SURAT")
        var providerKey = request.ShippingProvider.ToUpperInvariant();
        if (_shippingSettings.Providers.TryGetValue(providerKey, out var providerConfig))
        {
            baseCost = providerConfig.BaseCost;
        }
        else
        {
            // Provider name'e göre de kontrol et (örn: "Yurtiçi Kargo")
            var providerByName = _shippingSettings.Providers.Values
                .FirstOrDefault(p => p.Name.Equals(request.ShippingProvider, StringComparison.OrdinalIgnoreCase));
            if (providerByName != null)
            {
                baseCost = providerByName.BaseCost;
            }
        }

        // Ücretsiz kargo kontrolü
        if (order.SubTotal >= _orderSettings.FreeShippingThreshold)
        {
            return 0;
        }

        // Ağırlık veya hacim bazlı hesaplama yapılabilir
        // Şimdilik sadece base cost döndürüyoruz
        return baseCost;
    }
}

