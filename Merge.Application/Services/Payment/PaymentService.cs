using AutoMapper;
using PaymentEntity = Merge.Domain.Entities.Payment;
using OrderEntity = Merge.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Payment;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Application.DTOs.Payment;


namespace Merge.Application.Services.Payment;

public class PaymentService : IPaymentService
{
    private readonly IRepository<PaymentEntity> _paymentRepository;
    private readonly IRepository<OrderEntity> _orderRepository;
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IRepository<PaymentEntity> paymentRepository,
        IRepository<OrderEntity> orderRepository,
        IDbContext context,
        IMapper mapper,
        IUnitOfWork unitOfWork,
        ILogger<PaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _context = context;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaymentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving payment with ID: {PaymentId}", id);

            var payment = await _context.Set<PaymentEntity>()
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found with ID: {PaymentId}", id);
                return null;
            }

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            // Not: OrderNumber AutoMapper'da zaten map ediliyor
            return _mapper.Map<PaymentDto>(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment with ID: {PaymentId}", id);
            throw;
        }
    }

    public async Task<PaymentDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving payment for order ID: {OrderId}", orderId);

            var payment = await _context.Set<PaymentEntity>()
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found for order ID: {OrderId}", orderId);
                return null;
            }

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            // Not: OrderNumber AutoMapper'da zaten map ediliyor
            return _mapper.Map<PaymentDto>(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment for order ID: {OrderId}", orderId);
            throw;
        }
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (dto.Amount <= 0)
        {
            throw new ValidationException("Ödeme tutarı 0'dan büyük olmalıdır.");
        }

        try
        {
            _logger.LogInformation("Creating payment for order ID: {OrderId}, Amount: {Amount}, Method: {PaymentMethod}",
                dto.OrderId, dto.Amount, dto.PaymentMethod);

            await _unitOfWork.BeginTransactionAsync();

            var order = await _orderRepository.GetByIdAsync(dto.OrderId);
            if (order == null)
            {
                _logger.LogWarning("Order not found with ID: {OrderId}", dto.OrderId);
                throw new NotFoundException("Sipariş", dto.OrderId);
            }

            if (order.PaymentStatus == PaymentStatus.Completed)
            {
                _logger.LogWarning("Order {OrderId} is already paid", dto.OrderId);
                throw new BusinessException("Bu sipariş zaten ödenmiş.");
            }

            var existingPayment = await _context.Set<PaymentEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OrderId == dto.OrderId, cancellationToken);

            if (existingPayment != null)
            {
                _logger.LogWarning("Payment already exists for order ID: {OrderId}", dto.OrderId);
                throw new BusinessException("Bu sipariş için zaten bir ödeme kaydı var.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            var amount = new Money(dto.Amount);
            var payment = PaymentEntity.Create(
                dto.OrderId,
                dto.PaymentMethod,
                dto.PaymentProvider,
                amount
            );

            payment = await _paymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            payment = await _context.Set<PaymentEntity>()
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == payment.Id, cancellationToken);

            await _unitOfWork.CommitTransactionAsync();

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            // Not: OrderNumber AutoMapper'da zaten map ediliyor
            return _mapper.Map<PaymentDto>(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for order ID: {OrderId}", dto.OrderId);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<PaymentDto> ProcessPaymentAsync(Guid paymentId, ProcessPaymentDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        try
        {
            _logger.LogInformation("Processing payment with ID: {PaymentId}, TransactionId: {TransactionId}",
                paymentId, dto.TransactionId);

            await _unitOfWork.BeginTransactionAsync();

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found with ID: {PaymentId}", paymentId);
                throw new NotFoundException("Ödeme kaydı", paymentId);
            }

            if (payment.Status == PaymentStatus.Completed)
            {
                _logger.LogWarning("Payment {PaymentId} is already completed", paymentId);
                throw new BusinessException("Bu ödeme zaten tamamlanmış.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Burada gerçek payment gateway entegrasyonu yapılacak
            payment.Process();
            payment.Complete(dto.TransactionId, dto.PaymentReference);
            if (dto.Metadata != null)
            {
                payment.SetMetadata(System.Text.Json.JsonSerializer.Serialize(dto.Metadata));
            }

            await _paymentRepository.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Order payment status'unu güncelle
            var order = await _orderRepository.GetByIdAsync(payment.OrderId);
            if (order != null)
            {
                order.SetPaymentStatus(PaymentStatus.Completed);
                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated order {OrderId} payment status to Completed", payment.OrderId);
            }

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            payment = await _context.Set<PaymentEntity>()
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == payment.Id, cancellationToken);

            await _unitOfWork.CommitTransactionAsync();

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            // Not: OrderNumber AutoMapper'da zaten map ediliyor
            return _mapper.Map<PaymentDto>(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment with ID: {PaymentId}", paymentId);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<PaymentDto> RefundPaymentAsync(Guid paymentId, decimal? amount = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Initiating refund for payment ID: {PaymentId}, Refund Amount: {RefundAmount}",
                paymentId, amount?.ToString() ?? "Full");

            await _unitOfWork.BeginTransactionAsync();

            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                _logger.LogWarning("Payment not found with ID: {PaymentId}", paymentId);
                throw new NotFoundException("Ödeme kaydı", paymentId);
            }

            if (payment.Status != PaymentStatus.Completed)
            {
                _logger.LogWarning("Payment {PaymentId} cannot be refunded. Status: {Status}", paymentId, payment.Status);
                throw new BusinessException("Sadece tamamlanmış ödemeler iade edilebilir.");
            }

            var refundAmount = amount ?? payment.Amount;
            if (refundAmount <= 0)
            {
                _logger.LogWarning("Invalid refund amount {RefundAmount} for payment {PaymentId}", refundAmount, paymentId);
                throw new ValidationException("İade tutarı 0'dan büyük olmalıdır.");
            }

            if (refundAmount > payment.Amount)
            {
                _logger.LogWarning("Refund amount {RefundAmount} exceeds payment amount {PaymentAmount} for payment {PaymentId}",
                    refundAmount, payment.Amount, paymentId);
                throw new ValidationException("İade tutarı ödeme tutarından fazla olamaz.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Burada gerçek payment gateway refund işlemi yapılacak
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
            
            await _paymentRepository.UpdateAsync(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated payment {PaymentId} status to {Status}", paymentId, payment.Status);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            // Order payment status'unu güncelle
            var order = await _orderRepository.GetByIdAsync(payment.OrderId);
            if (order != null)
            {
                order.SetPaymentStatus(isFullRefund ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded);
                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Updated order {OrderId} payment status to {Status}", payment.OrderId, order.PaymentStatus);
            }

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            payment = await _context.Set<PaymentEntity>()
                .AsNoTracking()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == payment.Id, cancellationToken);

            await _unitOfWork.CommitTransactionAsync();

            // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
            // Not: OrderNumber AutoMapper'da zaten map ediliyor
            return _mapper.Map<PaymentDto>(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment with ID: {PaymentId}, Refund Amount: {RefundAmount}",
                paymentId, amount);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> VerifyPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Verifying payment with transaction ID: {TransactionId}", transactionId);

            var payment = await _context.Set<PaymentEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found with transaction ID: {TransactionId}", transactionId);
                return false;
            }

            // Burada payment gateway'den ödeme durumu sorgulanacak
            // Şimdilik sadece payment kaydının varlığını kontrol ediyoruz
            var isVerified = payment.Status == PaymentStatus.Completed;

            _logger.LogInformation("Payment verification result for transaction ID {TransactionId}: {IsVerified}",
                transactionId, isVerified);

            return isVerified;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying payment with transaction ID: {TransactionId}", transactionId);
            throw;
        }
    }
}

