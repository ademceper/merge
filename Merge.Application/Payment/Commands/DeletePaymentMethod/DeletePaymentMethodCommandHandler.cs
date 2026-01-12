using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Commands.DeletePaymentMethod;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class DeletePaymentMethodCommandHandler : IRequestHandler<DeletePaymentMethodCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePaymentMethodCommandHandler> _logger;

    public DeletePaymentMethodCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeletePaymentMethodCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeletePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting payment method. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);

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

            // Check if method is used in any orders
            var hasOrders = await _context.Set<OrderEntity>()
                .AsNoTracking()
                .AnyAsync(o => o.PaymentMethod == paymentMethod.Code, cancellationToken);

            if (hasOrders)
            {
                // Soft delete - just deactivate
                // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
                paymentMethod.Deactivate();
            }
            else
            {
                // Hard delete if no orders
                paymentMethod.MarkAsDeleted();
            }

            // paymentMethod.UpdatedAt = DateTime.UtcNow; // Handled by entity
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Payment method deleted successfully. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment method. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
