using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Commands.CreatePayment;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class CreatePaymentCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreatePaymentCommandHandler> logger) : IRequestHandler<CreatePaymentCommand, PaymentDto>
{

    public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Creating payment for order ID: {OrderId}, Amount: {Amount}, Method: {PaymentMethod}",
            request.OrderId, request.Amount, request.PaymentMethod);

        // CRITICAL: Transaction baslat - atomic operation
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // PERFORMANCE: AsNoTracking for read-only query (check icin)
            var order = await context.Set<OrderEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order is null)
            {
                logger.LogWarning("Order not found with ID: {OrderId}", request.OrderId);
                throw new NotFoundException("Siparis", request.OrderId);
            }

            if (order.PaymentStatus == PaymentStatus.Completed)
            {
                logger.LogWarning("Order {OrderId} is already paid", request.OrderId);
                throw new BusinessException("Bu siparis zaten odenmis.");
            }

            // PERFORMANCE: AsNoTracking for read-only query (check icin)
            var existingPayment = await context.Set<PaymentEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OrderId == request.OrderId, cancellationToken);

            if (existingPayment is not null)
            {
                logger.LogWarning("Payment already exists for order ID: {OrderId}", request.OrderId);
                throw new BusinessException("Bu siparis icin zaten bir odeme kaydi var.");
            }

            // BOLUM 1.1: Rich Domain Model - Factory method kullan
            var amount = new Money(request.Amount);
            var payment = PaymentEntity.Create(
                request.OrderId,
                request.PaymentMethod,
                request.PaymentProvider,
                amount
            );

            await context.Set<PaymentEntity>().AddAsync(payment, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            payment = await context.Set<PaymentEntity>()
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == payment.Id, cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation(
                "Payment created successfully. PaymentId: {PaymentId}, OrderId: {OrderId}, Amount: {Amount}",
                payment!.Id, request.OrderId, request.Amount);

            // ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return mapper.Map<PaymentDto>(payment);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating payment for order ID: {OrderId}", request.OrderId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
