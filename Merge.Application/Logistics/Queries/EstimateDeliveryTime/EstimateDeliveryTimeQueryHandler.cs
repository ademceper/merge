using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Logistics.Queries.EstimateDeliveryTime;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class EstimateDeliveryTimeQueryHandler : IRequestHandler<EstimateDeliveryTimeQuery, DeliveryTimeEstimateResultDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<EstimateDeliveryTimeQueryHandler> _logger;

    public EstimateDeliveryTimeQueryHandler(
        IDbContext context,
        ILogger<EstimateDeliveryTimeQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
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

        // If no estimation found, use default values
        if (estimation == null)
        {
            return new DeliveryTimeEstimateResultDto
            {
                MinDays = 3,
                MaxDays = 7,
                AverageDays = 5,
                EstimatedDeliveryDate = request.OrderDate.AddDays(5),
                EstimationSource = "System Default"
            };
        }

        var estimatedDate = request.OrderDate.AddDays(estimation.AverageDays);

        return new DeliveryTimeEstimateResultDto
        {
            MinDays = estimation.MinDays,
            MaxDays = estimation.MaxDays,
            AverageDays = estimation.AverageDays,
            EstimatedDeliveryDate = estimatedDate,
            EstimationSource = source ?? "Default"
        };
    }
}

