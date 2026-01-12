using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Payment;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Application.DTOs.Payment;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Payment;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentMethodService> _logger;

    public PaymentMethodService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<PaymentMethodService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PaymentMethodDto> CreatePaymentMethodAsync(CreatePaymentMethodDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Payment method oluşturuluyor. Name: {Name}, Code: {Code}, IsActive: {IsActive}",
            dto.Name, dto.Code, dto.IsActive);

        // ✅ PERFORMANCE: Removed manual !pm.IsDeleted (Global Query Filter)
        // Check if code already exists
        var existing = await _context.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.Code == dto.Code, cancellationToken);

        if (existing != null)
        {
            throw new BusinessException($"Bu kod ile ödeme yöntemi zaten mevcut: '{dto.Code}'");
        }

        // ✅ PERFORMANCE: Removed manual !pm.IsDeleted (Global Query Filter)
        // If this is default, unset other default methods
        if (dto.IsDefault)
        {
            var existingDefault = await _context.Set<PaymentMethod>()
                .Where(pm => pm.IsDefault)
                .ToListAsync(cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            foreach (var method in existingDefault)
            {
                method.UnsetAsDefault();
            }
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var settingsJson = dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null;
        
        var paymentMethod = PaymentMethod.Create(
            dto.Name,
            dto.Code,
            dto.Description,
            dto.IconUrl,
            dto.IsActive,
            dto.RequiresOnlinePayment,
            dto.RequiresManualVerification,
            dto.MinimumAmount,
            dto.MaximumAmount,
            dto.ProcessingFee,
            dto.ProcessingFeePercentage,
            settingsJson,
            dto.DisplayOrder,
            dto.IsDefault);

        await _context.Set<PaymentMethod>().AddAsync(paymentMethod, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation(
            "Payment method oluşturuldu. PaymentMethodId: {PaymentMethodId}, Name: {Name}, Code: {Code}",
            paymentMethod.Id, dto.Name, dto.Code);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<PaymentMethodDto>(paymentMethod);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pm.IsDeleted (Global Query Filter)
        var paymentMethod = await _context.Set<PaymentMethod>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return paymentMethod != null ? _mapper.Map<PaymentMethodDto>(paymentMethod) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PaymentMethodDto?> GetPaymentMethodByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pm.IsDeleted (Global Query Filter)
        var paymentMethod = await _context.Set<PaymentMethod>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pm => pm.Code == code && pm.IsActive, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return paymentMethod != null ? _mapper.Map<PaymentMethodDto>(paymentMethod) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<PaymentMethodDto>> GetAllPaymentMethodsAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pm.IsDeleted (Global Query Filter)
        var query = _context.Set<PaymentMethod>()
            .AsNoTracking()
            .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(pm => pm.IsActive == isActive.Value);
        }

        var methods = await query
            .OrderBy(pm => pm.DisplayOrder)
            .ThenBy(pm => pm.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select(MapToDto) YASAK - AutoMapper kullan
        return _mapper.Map<IEnumerable<PaymentMethodDto>>(methods);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<PaymentMethodDto>> GetAvailablePaymentMethodsAsync(decimal orderAmount, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pm.IsDeleted (Global Query Filter)
        var methods = await _context.Set<PaymentMethod>()
            .AsNoTracking()
            .Where(pm => pm.IsActive &&
                  (!pm.MinimumAmount.HasValue || orderAmount >= pm.MinimumAmount.Value) &&
                  (!pm.MaximumAmount.HasValue || orderAmount <= pm.MaximumAmount.Value))
            .OrderBy(pm => pm.DisplayOrder)
            .ThenBy(pm => pm.Name)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        // ✅ PERFORMANCE: ToListAsync() sonrası Select(MapToDto) YASAK - AutoMapper kullan
        return _mapper.Map<IEnumerable<PaymentMethodDto>>(methods);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdatePaymentMethodAsync(Guid id, UpdatePaymentMethodDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !pm.IsDeleted (Global Query Filter)
        var paymentMethod = await _context.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);

        if (paymentMethod == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        var settingsJson = dto.Settings != null ? JsonSerializer.Serialize(dto.Settings) : null;
        
        paymentMethod.Update(
            dto.Name,
            dto.Description,
            dto.IconUrl,
            dto.IsActive,
            dto.RequiresOnlinePayment,
            dto.RequiresManualVerification,
            dto.MinimumAmount,
            dto.MaximumAmount,
            dto.ProcessingFee,
            dto.ProcessingFeePercentage,
            settingsJson,
            dto.DisplayOrder);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (dto.IsDefault.HasValue && dto.IsDefault.Value)
        {
            // ✅ PERFORMANCE: Removed manual !pm.IsDeleted (Global Query Filter)
            // Unset other default methods
            var existingDefault = await _context.Set<PaymentMethod>()
                .Where(pm => pm.IsDefault && pm.Id != id)
                .ToListAsync(cancellationToken);

            foreach (var method in existingDefault)
            {
                method.UnsetAsDefault();
            }

            paymentMethod.SetAsDefault();
        }
        else if (dto.IsDefault.HasValue && !dto.IsDefault.Value)
        {
            paymentMethod.UnsetAsDefault();
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeletePaymentMethodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !pm.IsDeleted (Global Query Filter)
        var paymentMethod = await _context.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);

        if (paymentMethod == null) return false;

        // ✅ PERFORMANCE: Removed manual !o.IsDeleted (Global Query Filter)
        // Check if method is used in any orders
        var hasOrders = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .AnyAsync(o => o.PaymentMethod == paymentMethod.Code, cancellationToken);

        if (hasOrders)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            // Soft delete - just mark as inactive
            paymentMethod.Deactivate();
        }
        else
        {
            // Hard delete if no orders
            paymentMethod.IsDeleted = true;
        }

        paymentMethod.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> SetDefaultPaymentMethodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !pm.IsDeleted (Global Query Filter)
        var paymentMethod = await _context.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);

        if (paymentMethod == null) return false;

        // ✅ PERFORMANCE: Removed manual !pm.IsDeleted (Global Query Filter)
        // Unset other default methods
        var existingDefault = await _context.Set<PaymentMethod>()
            .Where(pm => pm.IsDefault && pm.Id != id)
            .ToListAsync(cancellationToken);

        foreach (var method in existingDefault)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            method.UnsetAsDefault();
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        paymentMethod.SetAsDefault();
        paymentMethod.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<decimal> CalculateProcessingFeeAsync(Guid paymentMethodId, decimal amount, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !pm.IsDeleted (Global Query Filter)
        var paymentMethod = await _context.Set<PaymentMethod>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pm => pm.Id == paymentMethodId && pm.IsActive, cancellationToken);

        if (paymentMethod == null)
        {
            throw new NotFoundException("Ödeme yöntemi", paymentMethodId);
        }

        var fee = paymentMethod.ProcessingFee ?? 0;
        if (paymentMethod.ProcessingFeePercentage.HasValue)
        {
            fee += amount * (paymentMethod.ProcessingFeePercentage.Value / 100);
        }

        return Math.Round(fee, 2);
    }
}

