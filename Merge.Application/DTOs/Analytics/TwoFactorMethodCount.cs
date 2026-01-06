namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record TwoFactorMethodCount(
    string Method,
    int Count,
    decimal Percentage
);

