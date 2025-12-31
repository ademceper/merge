using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.International;
using Merge.Application.DTOs.International;


namespace Merge.API.Controllers.International;

[ApiController]
[Route("api/international/currencies")]
public class CurrenciesController : BaseController
{
    private readonly ICurrencyService _currencyService;

    public CurrenciesController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    /// <summary>
    /// Get all currencies
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CurrencyDto>>> GetAllCurrencies()
    {
        var currencies = await _currencyService.GetAllCurrenciesAsync();
        return Ok(currencies);
    }

    /// <summary>
    /// Get active currencies only
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<CurrencyDto>>> GetActiveCurrencies()
    {
        var currencies = await _currencyService.GetActiveCurrenciesAsync();
        return Ok(currencies);
    }

    /// <summary>
    /// Get currency by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CurrencyDto>> GetCurrencyById(Guid id)
    {
        var currency = await _currencyService.GetCurrencyByIdAsync(id);

        if (currency == null)
        {
            return NotFound();
        }

        return Ok(currency);
    }

    /// <summary>
    /// Get currency by code
    /// </summary>
    [HttpGet("code/{code}")]
    public async Task<ActionResult<CurrencyDto>> GetCurrencyByCode(string code)
    {
        var currency = await _currencyService.GetCurrencyByCodeAsync(code);

        if (currency == null)
        {
            return NotFound();
        }

        return Ok(currency);
    }

    /// <summary>
    /// Create a new currency (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CurrencyDto>> CreateCurrency([FromBody] CreateCurrencyDto dto)
    {
        var currency = await _currencyService.CreateCurrencyAsync(dto);
        return CreatedAtAction(nameof(GetCurrencyById), new { id = currency.Id }, currency);
    }

    /// <summary>
    /// Update currency (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CurrencyDto>> UpdateCurrency(Guid id, [FromBody] UpdateCurrencyDto dto)
    {
        var currency = await _currencyService.UpdateCurrencyAsync(id, dto);
        if (currency == null)
        {
            return NotFound();
        }
        return Ok(currency);
    }

    /// <summary>
    /// Delete currency (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCurrency(Guid id)
    {
        await _currencyService.DeleteCurrencyAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Update exchange rate (Admin only)
    /// </summary>
    [HttpPut("{currencyCode}/exchange-rate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateExchangeRate(
        string currencyCode,
        [FromBody] decimal newRate,
        [FromQuery] string source = "Manual")
    {
        await _currencyService.UpdateExchangeRateAsync(currencyCode, newRate, source);
        return NoContent();
    }

    /// <summary>
    /// Convert price between currencies
    /// </summary>
    [HttpPost("convert")]
    public async Task<ActionResult<ConvertedPriceDto>> ConvertPrice([FromBody] ConvertPriceDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var result = await _currencyService.ConvertPriceAsync(dto.Amount, dto.FromCurrency, dto.ToCurrency);
        return Ok(result);
    }

    /// <summary>
    /// Format price in specific currency
    /// </summary>
    [HttpGet("format")]
    public async Task<ActionResult<string>> FormatPrice(
        [FromQuery] decimal amount,
        [FromQuery] string currencyCode)
    {
        var formatted = await _currencyService.FormatPriceAsync(amount, currencyCode);
        return Ok(new { formatted });
    }

    /// <summary>
    /// Get exchange rate history
    /// </summary>
    [HttpGet("{currencyCode}/history")]
    public async Task<ActionResult<IEnumerable<ExchangeRateHistoryDto>>> GetExchangeRateHistory(
        string currencyCode,
        [FromQuery] int days = 30)
    {
        var history = await _currencyService.GetExchangeRateHistoryAsync(currencyCode, days);
        return Ok(history);
    }

    /// <summary>
    /// Set user's currency preference
    /// </summary>
    [HttpPost("preference")]
    [Authorize]
    public async Task<IActionResult> SetCurrencyPreference([FromBody] string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            return BadRequest("Para birimi kodu boş olamaz.");
        }

        var userId = GetUserId();
        await _currencyService.SetUserCurrencyPreferenceAsync(userId, currencyCode);
        return NoContent();
    }

    /// <summary>
    /// Get user's currency preference
    /// </summary>
    [HttpGet("preference")]
    [Authorize]
    public async Task<ActionResult<string>> GetCurrencyPreference()
    {
        var userId = GetUserId();
        var currencyCode = await _currencyService.GetUserCurrencyPreferenceAsync(userId);
        return Ok(new { currencyCode });
    }

    /// <summary>
    /// Get currency statistics (Admin only)
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<CurrencyStatsDto>> GetCurrencyStats()
    {
        var stats = await _currencyService.GetCurrencyStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Sync exchange rates from external API (Admin only)
    /// </summary>
    [HttpPost("sync")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SyncExchangeRates()
    {
        await _currencyService.SyncExchangeRatesAsync();
        return NoContent();
    }
}
