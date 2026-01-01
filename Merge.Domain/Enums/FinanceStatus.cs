namespace Merge.Domain.Enums;

/// <summary>
/// Transaction status values for financial operations
/// </summary>
public enum FinanceTransactionStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3
}

/// <summary>
/// Invoice status values
/// </summary>
public enum SellerInvoiceStatus
{
    Draft = 0,
    Sent = 1,
    Paid = 2
}
