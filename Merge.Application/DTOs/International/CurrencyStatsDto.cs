namespace Merge.Application.DTOs.International;

public class CurrencyStatsDto
{
    public int TotalCurrencies { get; set; }
    public int ActiveCurrencies { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public DateTime LastRateUpdate { get; set; }
    public List<CurrencyUsageDto> MostUsedCurrencies { get; set; } = new();
}
