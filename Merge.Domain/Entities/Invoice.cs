namespace Merge.Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid OrderId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Draft"; // Draft, Sent, Paid, Cancelled
    public string? PdfUrl { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public Order Order { get; set; } = null!;
}

