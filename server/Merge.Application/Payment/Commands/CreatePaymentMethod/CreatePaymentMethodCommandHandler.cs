using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Text.Json;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Commands.CreatePaymentMethod;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class CreatePaymentMethodCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreatePaymentMethodCommandHandler> logger) : IRequestHandler<CreatePaymentMethodCommand, PaymentMethodDto>
{

    public async Task<PaymentMethodDto> Handle(CreatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Creating payment method. Name: {Name}, Code: {Code}, IsActive: {IsActive}",
            request.Name, request.Code, request.IsActive);

        // CRITICAL: Transaction baslat - atomic operation
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Check if code already exists
            var existing = await context.Set<PaymentMethod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(pm => pm.Code == request.Code, cancellationToken);

            if (existing != null)
            {
                logger.LogWarning("Payment method with code {Code} already exists", request.Code);
                throw new BusinessException($"Bu kod ile ödeme yöntemi zaten mevcut: '{request.Code}'");
            }

            // If this is default, unset other default methods
            if (request.IsDefault)
            {
                var existingDefault = await context.Set<PaymentMethod>()
                    .Where(pm => pm.IsDefault)
                    .ToListAsync(cancellationToken);

                foreach (var method in existingDefault)
                {
                    method.UnsetAsDefault();
                }
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            var settingsJson = request.Settings != null ? JsonSerializer.Serialize(request.Settings) : null;
            var paymentMethod = PaymentMethod.Create(
                request.Name,
                request.Code,
                request.Description,
                request.IconUrl,
                request.IsActive,
                request.RequiresOnlinePayment,
                request.RequiresManualVerification,
                request.MinimumAmount,
                request.MaximumAmount,
                request.ProcessingFee,
                request.ProcessingFeePercentage,
                settingsJson,
                request.DisplayOrder,
                request.IsDefault);

            await context.Set<PaymentMethod>().AddAsync(paymentMethod, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation(
                "Payment method created successfully. PaymentMethodId: {PaymentMethodId}, Name: {Name}, Code: {Code}",
                paymentMethod.Id, request.Name, request.Code);

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return mapper.Map<PaymentMethodDto>(paymentMethod);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating payment method. Name: {Name}, Code: {Code}", request.Name, request.Code);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
