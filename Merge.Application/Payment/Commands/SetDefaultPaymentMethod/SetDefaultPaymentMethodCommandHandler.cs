using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Commands.SetDefaultPaymentMethod;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class SetDefaultPaymentMethodCommandHandler : IRequestHandler<SetDefaultPaymentMethodCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SetDefaultPaymentMethodCommandHandler> _logger;

    public SetDefaultPaymentMethodCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<SetDefaultPaymentMethodCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(SetDefaultPaymentMethodCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Setting default payment method. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);

        // CRITICAL: Transaction baslat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var paymentMethod = await _context.Set<PaymentMethod>()
                .FirstOrDefaultAsync(pm => pm.Id == request.PaymentMethodId, cancellationToken);

            if (paymentMethod == null)
            {
                _logger.LogWarning("Payment method not found. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
                return false;
            }

            // Unset other default methods
            var existingDefault = await _context.Set<PaymentMethod>()
                .Where(pm => pm.IsDefault && pm.Id != request.PaymentMethodId)
                .ToListAsync(cancellationToken);

            foreach (var method in existingDefault)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
                method.UnsetAsDefault();
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            paymentMethod.SetAsDefault();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Default payment method set successfully. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default payment method. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
