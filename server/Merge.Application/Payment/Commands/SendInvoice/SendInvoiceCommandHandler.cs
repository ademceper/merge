using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Commands.SendInvoice;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class SendInvoiceCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<SendInvoiceCommandHandler> logger) : IRequestHandler<SendInvoiceCommand, bool>
{

    public async Task<bool> Handle(SendInvoiceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending invoice. InvoiceId: {InvoiceId}", request.InvoiceId);

        // CRITICAL: Transaction baslat - atomic operation
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var invoice = await context.Set<Invoice>()
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

            if (invoice is null)
            {
                logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", request.InvoiceId);
                return false;
            }

            invoice.MarkAsSent();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            logger.LogInformation("Invoice sent successfully. InvoiceId: {InvoiceId}", request.InvoiceId);

            // Email gönderilebilir (EmailService ile) - Event handler'da yapılabilir
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending invoice. InvoiceId: {InvoiceId}", request.InvoiceId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
