namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record TopCustomerDto(
    Guid CustomerId,
    string CustomerName,
    string Email,
    int OrderCount,
    decimal TotalSpent,
    DateTime LastOrderDate
);
