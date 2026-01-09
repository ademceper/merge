namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record ConvertedPriceDto(
    decimal OriginalAmount,
    string FromCurrency,
    decimal ConvertedAmount,
    string ToCurrency,
    string FormattedPrice,
    decimal ExchangeRate);
