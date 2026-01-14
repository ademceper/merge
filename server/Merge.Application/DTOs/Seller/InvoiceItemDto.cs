namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record InvoiceItemDto
{
    public string Description { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
    public Guid? CommissionId { get; init; }
    public Guid? OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public decimal CommissionAmount { get; init; }
    public decimal PlatformFee { get; init; }
    public decimal NetAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}
