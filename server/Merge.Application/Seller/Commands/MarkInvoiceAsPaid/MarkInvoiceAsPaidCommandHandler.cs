using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.MarkInvoiceAsPaid;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class MarkInvoiceAsPaidCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<MarkInvoiceAsPaidCommandHandler> logger) : IRequestHandler<MarkInvoiceAsPaidCommand, bool>
{

    public async Task<bool> Handle(MarkInvoiceAsPaidCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Marking invoice as paid. InvoiceId: {InvoiceId}", request.InvoiceId);

        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var invoice = await context.Set<SellerInvoice>()
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null)
        {
            logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", request.InvoiceId);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        invoice.MarkAsPaid();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Invoice marked as paid. InvoiceId: {InvoiceId}", request.InvoiceId);

        return true;
    }
}
