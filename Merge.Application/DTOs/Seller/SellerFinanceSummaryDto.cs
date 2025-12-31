namespace Merge.Application.DTOs.Seller;

public class SellerFinanceSummaryDto
{
    public Guid SellerId { get; set; }
    public SellerBalanceDto Balance { get; set; } = null!;
    public List<SellerTransactionDto> RecentTransactions { get; set; } = new();
    public List<SellerInvoiceDto> RecentInvoices { get; set; } = new();
    public Dictionary<string, decimal> EarningsByMonth { get; set; } = new();
    public Dictionary<string, decimal> PayoutsByMonth { get; set; } = new();
}
