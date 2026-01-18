using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Commands.SendInvoice;

public class SendInvoiceCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<SendInvoiceCommandHandler> logger) : IRequestHandler<SendInvoiceCommand, bool>
{

    public async Task<bool> Handle(SendInvoiceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending seller invoice. InvoiceId: {InvoiceId}", request.InvoiceId);

        var invoice = await context.Set<SellerInvoice>()
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null)
        {
            logger.LogWarning("Seller invoice not found. InvoiceId: {InvoiceId}", request.InvoiceId);
            throw new NotFoundException("Fatura", request.InvoiceId);
        }

        invoice.Send();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seller invoice sent successfully. InvoiceId: {InvoiceId}, InvoiceNumber: {InvoiceNumber}",
            request.InvoiceId, invoice.InvoiceNumber);

        return true;
    }
}
