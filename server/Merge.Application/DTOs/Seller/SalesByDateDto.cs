namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record SalesByDateDto
{
    public DateTime Date { get; init; }
    public decimal Sales { get; init; }
    public int OrderCount { get; init; }
}
