using Merge.Application.DTOs.Payment;
using Merge.Application.Common;
using Merge.Domain.Modules.Payment;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Payment;

public interface IInvoiceService
{
    Task<InvoiceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InvoiceDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<PagedResult<InvoiceDto>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<InvoiceDto> GenerateInvoiceAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<bool> SendInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    Task<string> GenerateInvoicePdfAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}

