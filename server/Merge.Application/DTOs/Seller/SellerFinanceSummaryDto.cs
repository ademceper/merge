namespace Merge.Application.DTOs.Seller;

public record SellerFinanceSummaryDto
{
    public Guid SellerId { get; init; }
    public SellerBalanceDto Balance { get; init; } = null!;
    public List<SellerTransactionDto> RecentTransactions { get; init; } = new();
    public List<SellerInvoiceDto> RecentInvoices { get; init; } = new();
    public Dictionary<string, decimal> EarningsByMonth { get; init; } = new();
    public Dictionary<string, decimal> PayoutsByMonth { get; init; } = new();
}
