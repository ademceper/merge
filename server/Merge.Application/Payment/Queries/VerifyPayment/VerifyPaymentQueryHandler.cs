using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Enums;
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Queries.VerifyPayment;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class VerifyPaymentQueryHandler : IRequestHandler<VerifyPaymentQuery, bool>
{
    private readonly IDbContext _context;
    private readonly ILogger<VerifyPaymentQueryHandler> _logger;

    public VerifyPaymentQueryHandler(
        IDbContext context,
        ILogger<VerifyPaymentQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> Handle(VerifyPaymentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Verifying payment with transaction ID: {TransactionId}", request.TransactionId);

        // ✅ PERFORMANCE: AsNoTracking for read-only query
        var payment = await _context.Set<PaymentEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.TransactionId == request.TransactionId, cancellationToken);

        if (payment == null)
        {
            _logger.LogWarning("Payment not found with transaction ID: {TransactionId}", request.TransactionId);
            return false;
        }

        // Burada payment gateway'den ödeme durumu sorgulanacak
        // Şimdilik sadece payment kaydının varlığını ve Completed status'unu kontrol ediyoruz
        var isVerified = payment.Status == PaymentStatus.Completed;

        _logger.LogInformation("Payment verification result for transaction ID {TransactionId}: {IsVerified}",
            request.TransactionId, isVerified);

        return isVerified;
    }
}
