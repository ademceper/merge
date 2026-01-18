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

public class MarkInvoiceAsPaidCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<MarkInvoiceAsPaidCommandHandler> logger) : IRequestHandler<MarkInvoiceAsPaidCommand, bool>
{

    public async Task<bool> Handle(MarkInvoiceAsPaidCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Marking invoice as paid. InvoiceId: {InvoiceId}", request.InvoiceId);

        var invoice = await context.Set<SellerInvoice>()
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice is null)
        {
            logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", request.InvoiceId);
            return false;
        }

        invoice.MarkAsPaid();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Invoice marked as paid. InvoiceId: {InvoiceId}", request.InvoiceId);

        return true;
    }
}
