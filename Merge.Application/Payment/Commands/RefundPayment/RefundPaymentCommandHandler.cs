using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using PaymentEntity = Merge.Domain.Entities.Payment;
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Application.Payment.Commands.RefundPayment;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, PaymentDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;

    public RefundPaymentCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<RefundPaymentCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaymentDto> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Initiating refund for payment ID: {PaymentId}, Refund Amount: {RefundAmount}",
            request.PaymentId, request.Amount?.ToString() ?? "Full");

        // CRITICAL: Transaction baslat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var payment = await _context.Set<PaymentEntity>()
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found with ID: {PaymentId}", request.PaymentId);
                throw new NotFoundException("Odeme kaydi", request.PaymentId);
            }

            if (payment.Status != PaymentStatus.Completed)
            {
                _logger.LogWarning("Payment {PaymentId} cannot be refunded. Status: {Status}", request.PaymentId, payment.Status);
                throw new BusinessException("Sadece tamamlanmis odemeler iade edilebilir.");
            }

            var refundAmount = request.Amount ?? payment.Amount;

            if (refundAmount > payment.Amount)
            {
                _logger.LogWarning(
                    "Refund amount {RefundAmount} exceeds payment amount {PaymentAmount} for payment {PaymentId}",
                    refundAmount, payment.Amount, request.PaymentId);
                throw new ValidationException("Iade tutari odeme tutarindan fazla olamaz.");
            }

            // BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Burada gercek payment gateway refund islemi yapilacak
            var refundMoney = new Money(refundAmount);
            var isFullRefund = refundAmount == payment.Amount;

            if (isFullRefund)
            {
                payment.Refund();
            }
            else
            {
                payment.PartiallyRefund(refundMoney);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated payment {PaymentId} status to {Status}", request.PaymentId, payment.Status);

            // BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Order payment status'unu guncelle
            var order = await _context.Set<OrderEntity>()
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId, cancellationToken);

            if (order != null)
            {
                order.SetPaymentStatus(isFullRefund ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated order {OrderId} payment status to {Status}", payment.OrderId, order.PaymentStatus);
            }

            // PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            payment = await _context.Set<PaymentEntity>()
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == payment.Id, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Payment refunded successfully. PaymentId: {PaymentId}, RefundAmount: {RefundAmount}, IsFullRefund: {IsFullRefund}",
                request.PaymentId, refundAmount, isFullRefund);

            // ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            return _mapper.Map<PaymentDto>(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment with ID: {PaymentId}, Refund Amount: {RefundAmount}",
                request.PaymentId, request.Amount);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
