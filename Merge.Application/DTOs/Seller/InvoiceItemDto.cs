namespace Merge.Application.DTOs.Seller;

public class InvoiceItemDto
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public Guid? CommissionId { get; set; }
    public Guid? OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal CommissionAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal NetAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
