using Merge.Application.DTOs.Payment;

namespace Merge.Application.Interfaces.Payment;

public interface IInvoiceService
{
    Task<InvoiceDto?> GetByIdAsync(Guid id);
    Task<InvoiceDto?> GetByOrderIdAsync(Guid orderId);
    Task<IEnumerable<InvoiceDto>> GetByUserIdAsync(Guid userId);
    Task<InvoiceDto> GenerateInvoiceAsync(Guid orderId);
    Task<bool> SendInvoiceAsync(Guid invoiceId);
    Task<string> GenerateInvoicePdfAsync(Guid invoiceId);
}

