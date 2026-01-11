using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Payment.Queries.CalculateProcessingFee;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class CalculateProcessingFeeQueryHandler : IRequestHandler<CalculateProcessingFeeQuery, decimal>
{
    private readonly IDbContext _context;
    private readonly ILogger<CalculateProcessingFeeQueryHandler> _logger;

    public CalculateProcessingFeeQueryHandler(
        IDbContext context,
        ILogger<CalculateProcessingFeeQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<decimal> Handle(CalculateProcessingFeeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calculating processing fee. PaymentMethodId: {PaymentMethodId}, Amount: {Amount}",
            request.PaymentMethodId, request.Amount);

        // ✅ PERFORMANCE: AsNoTracking for read-only query
        var paymentMethod = await _context.Set<PaymentMethod>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pm => pm.Id == request.PaymentMethodId && pm.IsActive, cancellationToken);

        if (paymentMethod == null)
        {
            _logger.LogWarning("Payment method not found. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
            throw new NotFoundException("Ödeme yöntemi", request.PaymentMethodId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
        var fee = paymentMethod.CalculateProcessingFee(request.Amount);

        _logger.LogInformation("Processing fee calculated. PaymentMethodId: {PaymentMethodId}, Amount: {Amount}, Fee: {Fee}",
            request.PaymentMethodId, request.Amount, fee);

        return fee;
    }
}
