namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record ExpenseByTypeDto(
    string ExpenseType, // Shipping, Commission, Refund, Discount, etc.
    decimal Amount,
    decimal Percentage
);
