using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Queries.EstimateDeliveryTime;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class EstimateDeliveryTimeQueryHandler : IRequestHandler<EstimateDeliveryTimeQuery, DeliveryTimeEstimateResultDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<EstimateDeliveryTimeQueryHandler> _logger;
    private readonly ShippingSettings _shippingSettings;

    public EstimateDeliveryTimeQueryHandler(
        IDbContext context,
        ILogger<EstimateDeliveryTimeQueryHandler> logger,
        IOptions<ShippingSettings> shippingSettings)
    {
        _context = context;
        _logger = logger;
        _shippingSettings = shippingSettings.Value;
    }

    public async Task<DeliveryTimeEstimateResultDto> Handle(EstimateDeliveryTimeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Estimating delivery time. ProductId: {ProductId}, CategoryId: {CategoryId}, WarehouseId: {WarehouseId}",
            request.ProductId, request.CategoryId, request.WarehouseId);

        // Try to find most specific estimation
        DeliveryTimeEstimation? estimation = null;
        string? source = null;

        // 1. Product-specific estimation
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        if (request.ProductId.HasValue)
        {
            estimation = await _context.Set<DeliveryTimeEstimation>()
                .AsNoTracking()
                .Where(e => e.IsActive &&
                      e.ProductId == request.ProductId.Value &&
                      (request.WarehouseId == null || e.WarehouseId == request.WarehouseId) &&
                      (string.IsNullOrEmpty(request.City) || e.City == request.City) &&
                      (string.IsNullOrEmpty(request.Country) || e.Country == request.Country))
                .FirstOrDefaultAsync(cancellationToken);

            if (estimation != null)
            {
                source = "Product";
            }
        }

        // 2. Category-specific estimation
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        if (estimation == null && request.CategoryId.HasValue)
        {
            estimation = await _context.Set<DeliveryTimeEstimation>()
                .AsNoTracking()
                .Where(e => e.IsActive &&
                      e.CategoryId == request.CategoryId.Value &&
                      (request.WarehouseId == null || e.WarehouseId == request.WarehouseId) &&
                      (string.IsNullOrEmpty(request.City) || e.City == request.City) &&
                      (string.IsNullOrEmpty(request.Country) || e.Country == request.Country))
                .FirstOrDefaultAsync(cancellationToken);

            if (estimation != null)
            {
                source = "Category";
            }
        }

        // 3. Warehouse-specific estimation
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        if (estimation == null && request.WarehouseId.HasValue)
        {
            estimation = await _context.Set<DeliveryTimeEstimation>()
                .AsNoTracking()
                .Where(e => e.IsActive &&
                      e.WarehouseId == request.WarehouseId.Value &&
                      (string.IsNullOrEmpty(request.City) || e.City == request.City) &&
                      (string.IsNullOrEmpty(request.Country) || e.Country == request.Country))
                .FirstOrDefaultAsync(cancellationToken);

            if (estimation != null)
            {
                source = "Warehouse";
            }
        }

        // 4. Default estimation (no specific match)
        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        if (estimation == null)
        {
            estimation = await _context.Set<DeliveryTimeEstimation>()
                .AsNoTracking()
                .Where(e => e.IsActive &&
                      e.ProductId == null &&
                      e.CategoryId == null &&
                      e.WarehouseId == null &&
                      (string.IsNullOrEmpty(request.City) || e.City == request.City) &&
                      (string.IsNullOrEmpty(request.Country) || e.Country == request.Country))
                .FirstOrDefaultAsync(cancellationToken);

            if (estimation != null)
            {
                source = "Default";
            }
        }

        // ✅ CONFIGURATION: Hardcoded değer yerine configuration kullan (BEST_PRACTICES_ANALIZI.md - BOLUM 2.1.4)
        // If no estimation found, use default values from configuration
        if (estimation == null)
        {
            // Default değerler ShippingSettings'ten alınır
            var defaultMinDays = _shippingSettings.DefaultDeliveryTime.MinDays;
            var defaultMaxDays = _shippingSettings.DefaultDeliveryTime.MaxDays;
            var defaultAverageDays = _shippingSettings.DefaultDeliveryTime.AverageDays;
            
            // Provider'ların ortalama estimated days'ini hesapla (eğer provider varsa)
            if (_shippingSettings.Providers.Any())
            {
                var avgEstimatedDays = (int)Math.Round(_shippingSettings.Providers.Values.Average(p => p.EstimatedDays));
                // Configuration'dan gelen değerlerle provider ortalamasını birleştir
                defaultAverageDays = Math.Max(defaultAverageDays, avgEstimatedDays);
                defaultMinDays = Math.Max(defaultMinDays, Math.Max(1, avgEstimatedDays - 2));
                defaultMaxDays = Math.Max(defaultMaxDays, avgEstimatedDays + 2);
            }
            
            return new DeliveryTimeEstimateResultDto(
                MinDays: defaultMinDays,
                MaxDays: defaultMaxDays,
                AverageDays: defaultAverageDays,
                EstimatedDeliveryDate: request.OrderDate.AddDays(defaultAverageDays),
                EstimationSource: "System Default");
        }

        var estimatedDate = request.OrderDate.AddDays(estimation.AverageDays);

        return new DeliveryTimeEstimateResultDto(
            MinDays: estimation.MinDays,
            MaxDays: estimation.MaxDays,
            AverageDays: estimation.AverageDays,
            EstimatedDeliveryDate: estimatedDate,
            EstimationSource: source ?? "Default");
    }
}

