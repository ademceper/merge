using Merge.Application.DTOs.Seller;
using Merge.Application.Common;
using Merge.Domain.Enums;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Seller;

public interface ISellerFinanceService
{
    // Balance
    Task<SellerBalanceDto> GetSellerBalanceAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<decimal> GetAvailableBalanceAsync(Guid sellerId, CancellationToken cancellationToken = default);
    Task<decimal> GetPendingBalanceAsync(Guid sellerId, CancellationToken cancellationToken = default);
    
    // Transactions
    // ✅ ARCHITECTURE: Enum kullanımı (string TransactionType yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    Task<SellerTransactionDto> CreateTransactionAsync(Guid sellerId, SellerTransactionType transactionType, decimal amount, string description, Guid? relatedEntityId = null, string? relatedEntityType = null, CancellationToken cancellationToken = default);
    Task<SellerTransactionDto?> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<PagedResult<SellerTransactionDto>> GetSellerTransactionsAsync(Guid sellerId, SellerTransactionType? transactionType = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    
    // Invoices
    Task<SellerInvoiceDto> GenerateInvoiceAsync(CreateSellerInvoiceDto dto, CancellationToken cancellationToken = default);
    Task<SellerInvoiceDto?> GetInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    Task<PagedResult<SellerInvoiceDto>> GetSellerInvoicesAsync(Guid sellerId, SellerInvoiceStatus? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> MarkInvoiceAsPaidAsync(Guid invoiceId, CancellationToken cancellationToken = default);
    
    // Summary
    Task<SellerFinanceSummaryDto> GetSellerFinanceSummaryAsync(Guid sellerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

