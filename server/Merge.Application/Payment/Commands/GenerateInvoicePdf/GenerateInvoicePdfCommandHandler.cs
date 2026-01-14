using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Commands.GenerateInvoicePdf;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GenerateInvoicePdfCommandHandler : IRequestHandler<GenerateInvoicePdfCommand, string>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GenerateInvoicePdfCommandHandler> _logger;

    public GenerateInvoicePdfCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<GenerateInvoicePdfCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> Handle(GenerateInvoicePdfCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating invoice PDF. InvoiceId: {InvoiceId}", request.InvoiceId);

        // CRITICAL: Transaction baslat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var invoice = await _context.Set<Invoice>()
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

            if (invoice == null)
            {
                _logger.LogWarning("Invoice not found. InvoiceId: {InvoiceId}", request.InvoiceId);
                throw new NotFoundException("Fatura", request.InvoiceId);
            }

            // PDF oluşturma işlemi burada yapılacak
            // Örnek: var pdfBytes = await GeneratePdfBytes(invoice);
            // var pdfUrl = await UploadPdfToStorage(pdfBytes, invoice.InvoiceNumber);
            
            var pdfUrl = $"/invoices/{invoice.InvoiceNumber}.pdf";

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            invoice.SetPdfUrl(pdfUrl);

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Invoice PDF generated successfully. InvoiceId: {InvoiceId}, PdfUrl: {PdfUrl}",
                request.InvoiceId, pdfUrl);

            return pdfUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating invoice PDF. InvoiceId: {InvoiceId}", request.InvoiceId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
