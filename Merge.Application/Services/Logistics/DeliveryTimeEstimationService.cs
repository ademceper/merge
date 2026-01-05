using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderEntity = Merge.Domain.Entities.Order;
using Merge.Application.Interfaces.Logistics;
using Merge.Domain.Entities;
using Merge.Application.Interfaces;
using System.Text.Json;
using Merge.Application.DTOs.Logistics;


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
        var estimation = new DeliveryTimeEstimation
        {
            ProductId = dto.ProductId,
            CategoryId = dto.CategoryId,
            WarehouseId = dto.WarehouseId,
            ShippingProviderId = dto.ShippingProviderId,
            City = dto.City,
            Country = dto.Country,
            MinDays = dto.MinDays,
            MaxDays = dto.MaxDays,
            AverageDays = dto.AverageDays,
            IsActive = dto.IsActive,
            Conditions = dto.Conditions != null ? JsonSerializer.Serialize(dto.Conditions) : null
        };

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

        if (dto.MinDays.HasValue)
        {
            estimation.MinDays = dto.MinDays.Value;
        }

        if (dto.MaxDays.HasValue)
        {
            estimation.MaxDays = dto.MaxDays.Value;
        }

        if (dto.AverageDays.HasValue)
        {
            estimation.AverageDays = dto.AverageDays.Value;
        }

        if (dto.IsActive.HasValue)
        {
            estimation.IsActive = dto.IsActive.Value;
        }

        if (dto.Conditions != null)
        {
            estimation.Conditions = JsonSerializer.Serialize(dto.Conditions);
        }

        estimation.UpdatedAt = DateTime.UtcNow;
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
            return new DeliveryTimeEstimateResultDto
            {
                MinDays = 3,
                MaxDays = 7,
                AverageDays = 5,
                EstimatedDeliveryDate = dto.OrderDate.AddDays(5),
                EstimationSource = "System Default"
            };
        }

        var estimatedDate = dto.OrderDate.AddDays(estimation.AverageDays);

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

