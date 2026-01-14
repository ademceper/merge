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
public class SendInvoiceCommandHandler : IRequestHandler<SendInvoiceCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SendInvoiceCommandHandler> _logger;

    public SendInvoiceCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<SendInvoiceCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(SendInvoiceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending invoice. InvoiceId: {InvoiceId}", request.InvoiceId);

        // CRITICAL: Transaction baslat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var invoice = await _context.Set<Invoice>()
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

            if (invoice == null)
            {
                _logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", request.InvoiceId);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            invoice.MarkAsSent();

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Invoice sent successfully. InvoiceId: {InvoiceId}", request.InvoiceId);

            // Email gönderilebilir (EmailService ile) - Event handler'da yapılabilir
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending invoice. InvoiceId: {InvoiceId}", request.InvoiceId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
