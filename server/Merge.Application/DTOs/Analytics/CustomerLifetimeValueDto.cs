namespace Merge.Application.DTOs.Analytics;

/// <summary>
/// Customer Lifetime Value DTO - BOLUM 4.2: Sensitive Data Exposure (YASAK)
/// Typed DTO kullanılıyor (object yerine)
/// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
/// </summary>
public record CustomerLifetimeValueDto(
    Guid CustomerId,
    decimal LifetimeValue
);

