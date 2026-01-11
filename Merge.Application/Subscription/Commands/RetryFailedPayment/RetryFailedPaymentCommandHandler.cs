using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Subscription.Commands.RetryFailedPayment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RetryFailedPaymentCommandHandler : IRequestHandler<RetryFailedPaymentCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RetryFailedPaymentCommandHandler> _logger;

    public RetryFailedPaymentCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RetryFailedPaymentCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RetryFailedPaymentCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Retrying failed payment. PaymentId: {PaymentId}", request.PaymentId);

        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (update için)
        var payment = await _context.Set<SubscriptionPayment>()
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId && p.PaymentStatus == PaymentStatus.Failed, cancellationToken);

        if (payment == null)
        {
            throw new NotFoundException("Başarısız ödeme", request.PaymentId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        payment.Retry();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Failed payment retry initiated. PaymentId: {PaymentId}, RetryCount: {RetryCount}",
            payment.Id, payment.RetryCount);

        return true;
    }
}
