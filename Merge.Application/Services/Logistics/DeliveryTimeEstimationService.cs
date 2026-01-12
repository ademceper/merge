using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Application.Interfaces.Logistics;
using Merge.Domain.Entities;
using Merge.Application.Interfaces;
using System.Text.Json;
using Merge.Application.DTOs.Logistics;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Logistics;

public class DeliveryTimeEstimationService : IDeliveryTimeEstimationService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DeliveryTimeEstimationService> _logger;

    public DeliveryTimeEstimationService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<DeliveryTimeEstimationService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<DeliveryTimeEstimationDto> CreateEstimationAsync(CreateDeliveryTimeEstimationDto dto, CancellationToken cancellationToken = default)
    {
        // Factory method kullan
        var conditionsJson = dto.Conditions != null ? JsonSerializer.Serialize(dto.Conditions) : null;
        var estimation = DeliveryTimeEstimation.Create(
            dto.MinDays,
            dto.MaxDays,
            dto.AverageDays,
            dto.ProductId,
            dto.CategoryId,
            dto.WarehouseId,
            dto.ShippingProviderId,
            dto.City,
            dto.Country,
            conditionsJson,
            dto.IsActive);

        await _context.Set<DeliveryTimeEstimation>().AddAsync(estimation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload with includes in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted (Global Query Filter)
        var createdEstimation = await _context.Set<DeliveryTimeEstimation>()
            .AsNoTracking()
            .Include(e => e.Product)
            .Include(e => e.Category)
            .Include(e => e.Warehouse)
            .FirstOrDefaultAsync(e => e.Id == estimation.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<DeliveryTimeEstimationDto>(createdEstimation!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<DeliveryTimeEstimationDto?> GetEstimationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted (Global Query Filter)
        var estimation = await _context.Set<DeliveryTimeEstimation>()
            .AsNoTracking()
            .Include(e => e.Product)
            .Include(e => e.Category)
            .Include(e => e.Warehouse)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return estimation != null ? _mapper.Map<DeliveryTimeEstimationDto>(estimation) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<DeliveryTimeEstimationDto>> GetAllEstimationsAsync(Guid? productId = null, Guid? categoryId = null, Guid? warehouseId = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted (Global Query Filter)
        IQueryable<DeliveryTimeEstimation> query = _context.Set<DeliveryTimeEstimation>()
            .AsNoTracking()
            .Include(e => e.Product)
            .Include(e => e.Category)
            .Include(e => e.Warehouse);

        if (productId.HasValue)
        {
            query = query.Where(e => e.ProductId == productId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(e => e.CategoryId == categoryId.Value);
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(e => e.WarehouseId == warehouseId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(e => e.IsActive == isActive.Value);
        }

        var estimations = await query
            .OrderBy(e => e.AverageDays)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<DeliveryTimeEstimationDto>>(estimations);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateEstimationAsync(Guid id, UpdateDeliveryTimeEstimationDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted (Global Query Filter)
        var estimation = await _context.Set<DeliveryTimeEstimation>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (estimation == null) return false;

        // Domain method kullan
        if (dto.MinDays.HasValue || dto.MaxDays.HasValue || dto.AverageDays.HasValue)
        {
            estimation.UpdateDays(
                dto.MinDays ?? estimation.MinDays,
                dto.MaxDays ?? estimation.MaxDays,
                dto.AverageDays ?? estimation.AverageDays);
        }

        if (dto.Conditions != null)
        {
            estimation.UpdateConditions(JsonSerializer.Serialize(dto.Conditions));
        }

        if (dto.IsActive.HasValue && dto.IsActive.Value && !estimation.IsActive)
        {
            estimation.Activate();
        }
        else if (dto.IsActive.HasValue && !dto.IsActive.Value && estimation.IsActive)
        {
            estimation.Deactivate();
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeleteEstimationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Update operasyonu, AsNoTracking gerekli değil
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted (Global Query Filter)
        var estimation = await _context.Set<DeliveryTimeEstimation>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (estimation == null) return false;

        estimation.IsDeleted = true;
        estimation.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<DeliveryTimeEstimateResultDto> EstimateDeliveryTimeAsync(EstimateDeliveryTimeDto dto, CancellationToken cancellationToken = default)
    {
        // Try to find most specific estimation
        DeliveryTimeEstimation? estimation = null;
        string? source = null;

        // 1. Product-specific estimation
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted (Global Query Filter)
        if (dto.ProductId.HasValue)
        {
            estimation = await _context.Set<DeliveryTimeEstimation>()
                .AsNoTracking()
                .Where(e => e.IsActive &&
                      e.ProductId == dto.ProductId.Value &&
                      (dto.WarehouseId == null || e.WarehouseId == dto.WarehouseId) &&
                      (string.IsNullOrEmpty(dto.City) || e.City == dto.City) &&
                      (string.IsNullOrEmpty(dto.Country) || e.Country == dto.Country))
                .FirstOrDefaultAsync(cancellationToken);

            if (estimation != null)
            {
                source = "Product";
            }
        }

        // 2. Category-specific estimation
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted (Global Query Filter)
        if (estimation == null && dto.CategoryId.HasValue)
        {
            estimation = await _context.Set<DeliveryTimeEstimation>()
                .AsNoTracking()
                .Where(e => e.IsActive &&
                      e.CategoryId == dto.CategoryId.Value &&
                      (dto.WarehouseId == null || e.WarehouseId == dto.WarehouseId) &&
                      (string.IsNullOrEmpty(dto.City) || e.City == dto.City) &&
                      (string.IsNullOrEmpty(dto.Country) || e.Country == dto.Country))
                .FirstOrDefaultAsync(cancellationToken);

            if (estimation != null)
            {
                source = "Category";
            }
        }

        // 3. Warehouse-specific estimation
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted (Global Query Filter)
        if (estimation == null && dto.WarehouseId.HasValue)
        {
            estimation = await _context.Set<DeliveryTimeEstimation>()
                .AsNoTracking()
                .Where(e => e.IsActive &&
                      e.WarehouseId == dto.WarehouseId.Value &&
                      (string.IsNullOrEmpty(dto.City) || e.City == dto.City) &&
                      (string.IsNullOrEmpty(dto.Country) || e.Country == dto.Country))
                .FirstOrDefaultAsync(cancellationToken);

            if (estimation != null)
            {
                source = "Warehouse";
            }
        }

        // 4. Default estimation (no specific match)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !e.IsDeleted (Global Query Filter)
        if (estimation == null)
        {
            estimation = await _context.Set<DeliveryTimeEstimation>()
                .AsNoTracking()
                .Where(e => e.IsActive &&
                      e.ProductId == null &&
                      e.CategoryId == null &&
                      e.WarehouseId == null &&
                      (string.IsNullOrEmpty(dto.City) || e.City == dto.City) &&
                      (string.IsNullOrEmpty(dto.Country) || e.Country == dto.Country))
                .FirstOrDefaultAsync(cancellationToken);

            if (estimation != null)
            {
                source = "Default";
            }
        }

        // If no estimation found, use default values
        if (estimation == null)
        {
            return new DeliveryTimeEstimateResultDto(
                MinDays: 3,
                MaxDays: 7,
                AverageDays: 5,
                EstimatedDeliveryDate: dto.OrderDate.AddDays(5),
                EstimationSource: "System Default");
        }

        var estimatedDate = dto.OrderDate.AddDays(estimation.AverageDays);

        return new DeliveryTimeEstimateResultDto(
            MinDays: estimation.MinDays,
            MaxDays: estimation.MaxDays,
            AverageDays: estimation.AverageDays,
            EstimatedDeliveryDate: estimatedDate,
            EstimationSource: source ?? "Default");
    }

}

