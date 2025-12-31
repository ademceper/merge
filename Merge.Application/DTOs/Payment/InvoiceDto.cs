using Merge.Application.DTOs.User;
using Merge.Application.DTOs.Order;

namespace Merge.Application.DTOs.Payment;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PdfUrl { get; set; }
    public string? Notes { get; set; }
    public AddressDto? BillingAddress { get; set; }
    public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
}
