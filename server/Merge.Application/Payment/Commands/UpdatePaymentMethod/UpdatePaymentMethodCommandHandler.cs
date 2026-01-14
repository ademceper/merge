using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Commands.UpdatePaymentMethod;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class UpdatePaymentMethodCommandHandler : IRequestHandler<UpdatePaymentMethodCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePaymentMethodCommandHandler> _logger;

    public UpdatePaymentMethodCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePaymentMethodCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdatePaymentMethodCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating payment method. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);

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

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            var settingsJson = request.Settings != null ? JsonSerializer.Serialize(request.Settings) : null;
            paymentMethod.Update(
                request.Name,
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
                request.DisplayOrder);

            // Handle IsDefault separately (requires unsetting other defaults)
            if (request.IsDefault.HasValue && request.IsDefault.Value)
            {
                var existingDefault = await _context.Set<PaymentMethod>()
                    .Where(pm => pm.IsDefault && pm.Id != request.PaymentMethodId)
                    .ToListAsync(cancellationToken);

                foreach (var method in existingDefault)
                {
                    method.UnsetAsDefault();
                }

                paymentMethod.SetAsDefault();
            }
            else if (request.IsDefault.HasValue && !request.IsDefault.Value)
            {
                paymentMethod.UnsetAsDefault();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Payment method updated successfully. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment method. PaymentMethodId: {PaymentMethodId}", request.PaymentMethodId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
