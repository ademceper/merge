using Merge.Application.DTOs.Seller;
namespace Merge.Application.Interfaces.Seller;

public interface ISellerFinanceService
{
    // Balance
    Task<SellerBalanceDto> GetSellerBalanceAsync(Guid sellerId);
    Task<decimal> GetAvailableBalanceAsync(Guid sellerId);
    Task<decimal> GetPendingBalanceAsync(Guid sellerId);
    
    // Transactions
    Task<SellerTransactionDto> CreateTransactionAsync(Guid sellerId, string transactionType, decimal amount, string description, Guid? relatedEntityId = null, string? relatedEntityType = null);
    Task<SellerTransactionDto?> GetTransactionAsync(Guid transactionId);
    Task<IEnumerable<SellerTransactionDto>> GetSellerTransactionsAsync(Guid sellerId, string? transactionType = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20);
    
    // Invoices
    Task<SellerInvoiceDto> GenerateInvoiceAsync(CreateSellerInvoiceDto dto);
    Task<SellerInvoiceDto?> GetInvoiceAsync(Guid invoiceId);
    Task<IEnumerable<SellerInvoiceDto>> GetSellerInvoicesAsync(Guid sellerId, string? status = null, int page = 1, int pageSize = 20);
    Task<bool> MarkInvoiceAsPaidAsync(Guid invoiceId);
    
    // Summary
    Task<SellerFinanceSummaryDto> GetSellerFinanceSummaryAsync(Guid sellerId, DateTime? startDate = null, DateTime? endDate = null);
}

