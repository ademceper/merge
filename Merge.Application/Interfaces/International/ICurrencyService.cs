using Merge.Application.DTOs.International;

namespace Merge.Application.Interfaces.International;

public interface ICurrencyService
{
    Task<IEnumerable<CurrencyDto>> GetAllCurrenciesAsync();
    Task<IEnumerable<CurrencyDto>> GetActiveCurrenciesAsync();
    Task<CurrencyDto?> GetCurrencyByIdAsync(Guid id);
    Task<CurrencyDto?> GetCurrencyByCodeAsync(string code);
    Task<CurrencyDto> CreateCurrencyAsync(CreateCurrencyDto dto);
    Task<CurrencyDto> UpdateCurrencyAsync(Guid id, UpdateCurrencyDto dto);
    Task DeleteCurrencyAsync(Guid id);
    Task UpdateExchangeRateAsync(string currencyCode, decimal newRate, string source = "Manual");
    Task<ConvertedPriceDto> ConvertPriceAsync(decimal amount, string fromCurrency, string toCurrency);
    Task<string> FormatPriceAsync(decimal amount, string currencyCode);
    Task<IEnumerable<ExchangeRateHistoryDto>> GetExchangeRateHistoryAsync(string currencyCode, int days = 30);
    Task SetUserCurrencyPreferenceAsync(Guid userId, string currencyCode);
    Task<string> GetUserCurrencyPreferenceAsync(Guid userId);
    Task<CurrencyStatsDto> GetCurrencyStatsAsync();
    Task SyncExchangeRatesAsync(); // For future API integration
}
