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
public class CreateDeliveryTimeEstimationCommandHandler : IRequestHandler<CreateDeliveryTimeEstimationCommand, DeliveryTimeEstimationDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateDeliveryTimeEstimationCommandHandler> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public CreateDeliveryTimeEstimationCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateDeliveryTimeEstimationCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<DeliveryTimeEstimationDto> Handle(CreateDeliveryTimeEstimationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating delivery time estimation. ProductId: {ProductId}, CategoryId: {CategoryId}, WarehouseId: {WarehouseId}",
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

        await _context.Set<DeliveryTimeEstimation>().AddAsync(estimation, cancellationToken);
        
        // ✅ ARCHITECTURE: UnitOfWork kullan (Repository pattern)
        // ✅ ARCHITECTURE: Domain events are automatically dispatched and stored in OutboxMessages by UnitOfWork.SaveChangesAsync
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için cartesian explosion önleme
        var createdEstimation = await _context.Set<DeliveryTimeEstimation>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting (AsSplitQuery) - Cartesian explosion önleme
            .Include(e => e.Product)
            .Include(e => e.Category)
            .Include(e => e.Warehouse)
            .FirstOrDefaultAsync(e => e.Id == estimation.Id, cancellationToken);

        if (createdEstimation == null)
        {
            _logger.LogWarning("Delivery time estimation not found after creation. EstimationId: {EstimationId}", estimation.Id);
            throw new NotFoundException("Teslimat süresi tahmini", estimation.Id);
        }

        _logger.LogInformation("Delivery time estimation created successfully. EstimationId: {EstimationId}", estimation.Id);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<DeliveryTimeEstimationDto>(createdEstimation);
    }
}

