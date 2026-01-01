namespace Merge.Domain.Enums;

/// <summary>
/// Invoice status values for Invoice entity
/// </summary>
public enum InvoiceStatus
{
    Draft = 0,
    Sent = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4,
    PartiallyPaid = 5
}
