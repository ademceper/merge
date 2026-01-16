using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Logistics.Commands.CreateDeliveryTimeEstimation;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern C# feature kullanımı
public class CreateDeliveryTimeEstimationCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateDeliveryTimeEstimationCommandHandler> logger) : IRequestHandler<CreateDeliveryTimeEstimationCommand, DeliveryTimeEstimationDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<DeliveryTimeEstimationDto> Handle(CreateDeliveryTimeEstimationCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating delivery time estimation. ProductId: {ProductId}, CategoryId: {CategoryId}, WarehouseId: {WarehouseId}",
            request.ProductId, request.CategoryId, request.WarehouseId);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var conditionsJson = request.Conditions != null
            ? JsonSerializer.Serialize(request.Conditions, JsonOptions)
            : null;

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        // Factory method parametre sırası: minDays, maxDays, averageDays, productId, categoryId, warehouseId, shippingProviderId, city, country, conditions, isActive
        var estimation = DeliveryTimeEstimation.Create(
            request.MinDays,
            request.MaxDays,
            request.AverageDays,
            request.ProductId,
            request.CategoryId,
            request.WarehouseId,
            request.ShippingProviderId,
            request.City,
            request.Country,
            conditionsJson,
            request.IsActive);

        await context.Set<DeliveryTimeEstimation>().AddAsync(estimation, cancellationToken);
        
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdEstimation = await context.Set<DeliveryTimeEstimation>()
            .AsNoTracking()
            .Include(e => e.Product)
            .Include(e => e.Category)
            .Include(e => e.Warehouse)
            .FirstOrDefaultAsync(e => e.Id == estimation.Id, cancellationToken);

        if (createdEstimation == null)
        {
            logger.LogWarning("Delivery time estimation not found after creation. EstimationId: {EstimationId}", estimation.Id);
            throw new NotFoundException("Teslimat süresi tahmini", estimation.Id);
        }

        logger.LogInformation("Delivery time estimation created successfully. EstimationId: {EstimationId}", estimation.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<DeliveryTimeEstimationDto>(createdEstimation);
    }
}

