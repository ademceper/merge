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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
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
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Sending seller invoice. InvoiceId: {InvoiceId}", request.InvoiceId);

        // ✅ PERFORMANCE: Removed manual !i.IsDeleted (Global Query Filter)
        var invoice = await _context.Set<SellerInvoice>()
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null)
        {
            _logger.LogWarning("Seller invoice not found. InvoiceId: {InvoiceId}", request.InvoiceId);
            throw new NotFoundException("Fatura", request.InvoiceId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        invoice.Send();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seller invoice sent successfully. InvoiceId: {InvoiceId}, InvoiceNumber: {InvoiceNumber}",
            request.InvoiceId, invoice.InvoiceNumber);

        return true;
    }
}
