using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record CurrencyUsageDto(
    string CurrencyCode,
    string CurrencyName,
    int UserCount,
    decimal Percentage);
