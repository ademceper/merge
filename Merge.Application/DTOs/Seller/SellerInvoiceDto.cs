namespace Merge.Application.DTOs.Seller;

public class SellerInvoiceDto
{
    public Guid Id { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalCommissions { get; set; }
    public decimal TotalPayouts { get; set; }
    public decimal PlatformFees { get; set; }
    public decimal NetAmount { get; set; }
    public string Status { get; set; } = string.Empty; // Draft, Sent, Paid
    public DateTime? PaidAt { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
