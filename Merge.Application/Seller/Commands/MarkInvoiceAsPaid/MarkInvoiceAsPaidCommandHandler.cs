using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Commands.MarkInvoiceAsPaid;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class MarkInvoiceAsPaidCommandHandler : IRequestHandler<MarkInvoiceAsPaidCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkInvoiceAsPaidCommandHandler> _logger;

    public MarkInvoiceAsPaidCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<MarkInvoiceAsPaidCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(MarkInvoiceAsPaidCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Marking invoice as paid. InvoiceId: {InvoiceId}", request.InvoiceId);

        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var invoice = await _context.Set<SellerInvoice>()
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null)
        {
            _logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", request.InvoiceId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        invoice.MarkAsPaid();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice marked as paid. InvoiceId: {InvoiceId}", request.InvoiceId);

        return true;
    }
}
