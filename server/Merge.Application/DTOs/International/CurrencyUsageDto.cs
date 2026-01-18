using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.International;

public record CurrencyUsageDto(
    string CurrencyCode,
    string CurrencyName,
    int UserCount,
    decimal Percentage);
