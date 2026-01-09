namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record ExchangeRateHistoryDto(
    Guid Id,
    string CurrencyCode,
    decimal ExchangeRate,
    DateTime RecordedAt,
    string Source);
