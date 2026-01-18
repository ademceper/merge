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

        var conditionsJson = request.Conditions is not null
            ? JsonSerializer.Serialize(request.Conditions, JsonOptions)
            : null;

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
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdEstimation = await context.Set<DeliveryTimeEstimation>()
            .AsNoTracking()
            .Include(e => e.Product)
            .Include(e => e.Category)
            .Include(e => e.Warehouse)
            .FirstOrDefaultAsync(e => e.Id == estimation.Id, cancellationToken);

        if (createdEstimation is null)
        {
            logger.LogWarning("Delivery time estimation not found after creation. EstimationId: {EstimationId}", estimation.Id);
            throw new NotFoundException("Teslimat süresi tahmini", estimation.Id);
        }

        logger.LogInformation("Delivery time estimation created successfully. EstimationId: {EstimationId}", estimation.Id);

        return mapper.Map<DeliveryTimeEstimationDto>(createdEstimation);
    }
}

