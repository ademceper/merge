using Merge.Application.DTOs.International;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.International;

public interface ICurrencyService
{
    Task<IEnumerable<CurrencyDto>> GetAllCurrenciesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CurrencyDto>> GetActiveCurrenciesAsync(CancellationToken cancellationToken = default);
    Task<CurrencyDto?> GetCurrencyByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CurrencyDto?> GetCurrencyByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<CurrencyDto> CreateCurrencyAsync(CreateCurrencyDto dto, CancellationToken cancellationToken = default);
    Task<CurrencyDto> UpdateCurrencyAsync(Guid id, UpdateCurrencyDto dto, CancellationToken cancellationToken = default);
    Task DeleteCurrencyAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateExchangeRateAsync(string currencyCode, decimal newRate, string source = "Manual", CancellationToken cancellationToken = default);
    Task<ConvertedPriceDto> ConvertPriceAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default);
    Task<string> FormatPriceAsync(decimal amount, string currencyCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExchangeRateHistoryDto>> GetExchangeRateHistoryAsync(string currencyCode, int days = 30, CancellationToken cancellationToken = default);
    Task SetUserCurrencyPreferenceAsync(Guid userId, string currencyCode, CancellationToken cancellationToken = default);
    Task<string> GetUserCurrencyPreferenceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CurrencyStatsDto> GetCurrencyStatsAsync(CancellationToken cancellationToken = default);
    Task SyncExchangeRatesAsync(CancellationToken cancellationToken = default); // For future API integration
}
