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

public class PaymentMethodService(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<PaymentMethodService> logger) : IPaymentMethodService
{

    public async Task<PaymentMethodDto> CreatePaymentMethodAsync(CreatePaymentMethodDto dto, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Payment method oluşturuluyor. Name: {Name}, Code: {Code}, IsActive: {IsActive}",
            dto.Name, dto.Code, dto.IsActive);

        // Check if code already exists
        var existing = await context.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.Code == dto.Code, cancellationToken);

        if (existing != null)
        {
            throw new BusinessException($"Bu kod ile ödeme yöntemi zaten mevcut: '{dto.Code}'");
        }

        // If this is default, unset other default methods
        if (dto.IsDefault)
        {
            var existingDefault = await context.Set<PaymentMethod>()
                .Where(pm => pm.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var method in existingDefault)
            {
                method.UnsetAsDefault();
            }
        }

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

        await context.Set<PaymentMethod>().AddAsync(paymentMethod, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Payment method oluşturuldu. PaymentMethodId: {PaymentMethodId}, Name: {Name}, Code: {Code}",
            paymentMethod.Id, dto.Name, dto.Code);

        return mapper.Map<PaymentMethodDto>(paymentMethod);
    }

    public async Task<PaymentMethodDto?> GetPaymentMethodByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var paymentMethod = await context.Set<PaymentMethod>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);

        return paymentMethod != null ? mapper.Map<PaymentMethodDto>(paymentMethod) : null;
    }

    public async Task<PaymentMethodDto?> GetPaymentMethodByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var paymentMethod = await context.Set<PaymentMethod>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pm => pm.Code == code && pm.IsActive, cancellationToken);

        return paymentMethod != null ? mapper.Map<PaymentMethodDto>(paymentMethod) : null;
    }

    public async Task<IEnumerable<PaymentMethodDto>> GetAllPaymentMethodsAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = context.Set<PaymentMethod>()
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

        return mapper.Map<IEnumerable<PaymentMethodDto>>(methods);
    }

    public async Task<IEnumerable<PaymentMethodDto>> GetAvailablePaymentMethodsAsync(decimal orderAmount, CancellationToken cancellationToken = default)
    {
        var methods = await context.Set<PaymentMethod>()
            .AsNoTracking()
            .Where(pm => pm.IsActive &&
                  (!pm.MinimumAmount.HasValue || orderAmount >= pm.MinimumAmount.Value) &&
                  (!pm.MaximumAmount.HasValue || orderAmount <= pm.MaximumAmount.Value))
            .OrderBy(pm => pm.DisplayOrder)
            .ThenBy(pm => pm.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<PaymentMethodDto>>(methods);
    }

    public async Task<bool> UpdatePaymentMethodAsync(Guid id, UpdatePaymentMethodDto dto, CancellationToken cancellationToken = default)
    {
        var paymentMethod = await context.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);

        if (paymentMethod == null) return false;

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

        if (dto.IsDefault.HasValue && dto.IsDefault.Value)
        {
            // Unset other default methods
            var existingDefault = await context.Set<PaymentMethod>()
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
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeletePaymentMethodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var paymentMethod = await context.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);

        if (paymentMethod == null) return false;

        // Check if method is used in any orders
        var hasOrders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .AnyAsync(o => o.PaymentMethod == paymentMethod.Code, cancellationToken);

        if (hasOrders)
        {
            // Soft delete - just mark as inactive
            paymentMethod.Deactivate();
        }
        else
        {
            // Hard delete if no orders
            paymentMethod.MarkAsDeleted();
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> SetDefaultPaymentMethodAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var paymentMethod = await context.Set<PaymentMethod>()
            .FirstOrDefaultAsync(pm => pm.Id == id, cancellationToken);

        if (paymentMethod == null) return false;

        // Unset other default methods
        var existingDefault = await context.Set<PaymentMethod>()
            .Where(pm => pm.IsDefault && pm.Id != id)
            .ToListAsync(cancellationToken);

        foreach (var method in existingDefault)
        {
            method.UnsetAsDefault();
        }

        paymentMethod.SetAsDefault();
        // paymentMethod.UpdatedAt = DateTime.UtcNow; // Handled by SetAsDefault
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<decimal> CalculateProcessingFeeAsync(Guid paymentMethodId, decimal amount, CancellationToken cancellationToken = default)
    {
        var paymentMethod = await context.Set<PaymentMethod>()
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

