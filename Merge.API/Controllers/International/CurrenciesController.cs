using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.International;
using Merge.API.Middleware;
using Merge.Application.International.Queries.GetAllCurrencies;
using Merge.Application.International.Queries.GetActiveCurrencies;
using Merge.Application.International.Queries.GetCurrencyById;
using Merge.Application.International.Queries.GetCurrencyByCode;
using Merge.Application.International.Commands.CreateCurrency;
using Merge.Application.International.Commands.UpdateCurrency;
using Merge.Application.International.Commands.DeleteCurrency;
using Merge.Application.International.Commands.UpdateExchangeRate;
using Merge.Application.International.Commands.ConvertPrice;
using Merge.Application.International.Queries.FormatPrice;
using Merge.Application.International.Queries.GetExchangeRateHistory;
using Merge.Application.International.Commands.SetUserCurrencyPreference;
using Merge.Application.International.Queries.GetUserCurrencyPreference;
using Merge.Application.International.Queries.GetCurrencyStats;
using Merge.Application.International.Commands.SyncExchangeRates;

namespace Merge.API.Controllers.International;

// ✅ BOLUM 4.1: API Versioning (ZORUNLU)
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/international/currencies")]
public class CurrenciesController : BaseController
{
    private readonly IMediator _mediator;

    public CurrenciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Tüm para birimlerini getirir
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<CurrencyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<CurrencyDto>>> GetAllCurrencies(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetAllCurrenciesQuery();
        var currencies = await _mediator.Send(query, cancellationToken);
        return Ok(currencies);
    }

    /// <summary>
    /// Aktif para birimlerini getirir
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<CurrencyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<CurrencyDto>>> GetActiveCurrencies(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetActiveCurrenciesQuery();
        var currencies = await _mediator.Send(query, cancellationToken);
        return Ok(currencies);
    }

    /// <summary>
    /// Para birimi detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CurrencyDto>> GetCurrencyById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetCurrencyByIdQuery(id);
        var currency = await _mediator.Send(query, cancellationToken);

        if (currency == null)
        {
            return NotFound();
        }

        return Ok(currency);
    }

    /// <summary>
    /// Para birimi koduna göre getirir
    /// </summary>
    [HttpGet("code/{code}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CurrencyDto>> GetCurrencyByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetCurrencyByCodeQuery(code);
        var currency = await _mediator.Send(query, cancellationToken);

        if (currency == null)
        {
            return NotFound();
        }

        return Ok(currency);
    }

    /// <summary>
    /// Yeni para birimi oluşturur (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CurrencyDto>> CreateCurrency(
        [FromBody] CreateCurrencyDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.3: ValidationBehavior otomatik olarak ValidateModelState'i handle ediyor
        var command = new CreateCurrencyCommand(
            dto.Code,
            dto.Name,
            dto.Symbol,
            dto.ExchangeRate,
            dto.IsBaseCurrency,
            dto.IsActive,
            dto.DecimalPlaces,
            dto.Format);
        var currency = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCurrencyById), new { id = currency.Id }, currency);
    }

    /// <summary>
    /// Para birimini günceller (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CurrencyDto>> UpdateCurrency(
        Guid id,
        [FromBody] UpdateCurrencyDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.3: ValidationBehavior otomatik olarak ValidateModelState'i handle ediyor
        var command = new UpdateCurrencyCommand(
            id,
            dto.Name,
            dto.Symbol,
            dto.ExchangeRate,
            dto.IsActive,
            dto.DecimalPlaces,
            dto.Format);
        var currency = await _mediator.Send(command, cancellationToken);
        return Ok(currency);
    }

    /// <summary>
    /// Para birimini siler (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteCurrency(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new DeleteCurrencyCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Döviz kuru günceller (Admin only)
    /// </summary>
    [HttpPut("{currencyCode}/exchange-rate")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> UpdateExchangeRate(
        string currencyCode,
        [FromBody] decimal newRate,
        [FromQuery] string source = "Manual",
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new UpdateExchangeRateCommand(currencyCode, newRate, source);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Para birimleri arasında fiyat dönüştürür
    /// </summary>
    [HttpPost("convert")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ConvertedPriceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ConvertedPriceDto>> ConvertPrice(
        [FromBody] ConvertPriceDto dto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        // ✅ BOLUM 2.3: ValidationBehavior otomatik olarak ValidateModelState'i handle ediyor
        var command = new ConvertPriceCommand(dto.Amount, dto.FromCurrency, dto.ToCurrency);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Belirli para biriminde fiyat formatlar
    /// </summary>
    [HttpGet("format")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<string>> FormatPrice(
        [FromQuery] decimal amount,
        [FromQuery] string currencyCode,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new FormatPriceQuery(amount, currencyCode);
        var formatted = await _mediator.Send(query, cancellationToken);
        return Ok(new { formatted });
    }

    /// <summary>
    /// Döviz kuru geçmişini getirir
    /// </summary>
    [HttpGet("{currencyCode}/history")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ExchangeRateHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ExchangeRateHistoryDto>>> GetExchangeRateHistory(
        string currencyCode,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetExchangeRateHistoryQuery(currencyCode, days);
        var history = await _mediator.Send(query, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Kullanıcının para birimi tercihini ayarlar
    /// </summary>
    [HttpPost("preference")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SetCurrencyPreference(
        [FromBody] string currencyCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            return BadRequest("Para birimi kodu boş olamaz.");
        }

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi tercihini ayarlayabilir
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new SetUserCurrencyPreferenceCommand(userId, currencyCode);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Kullanıcının para birimi tercihini getirir
    /// </summary>
    [HttpGet("preference")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<string>> GetCurrencyPreference(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi tercihini görebilir
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetUserCurrencyPreferenceQuery(userId);
        var currencyCode = await _mediator.Send(query, cancellationToken);
        return Ok(new { currencyCode });
    }

    /// <summary>
    /// Para birimi istatistiklerini getirir (Admin only)
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(CurrencyStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CurrencyStatsDto>> GetCurrencyStats(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var query = new GetCurrencyStatsQuery();
        var stats = await _mediator.Send(query, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Döviz kurlarını harici API'den senkronize eder (Admin only)
    /// </summary>
    [HttpPost("sync")]
    [Authorize(Roles = "Admin")]
    [RateLimit(5, 60)] // ✅ BOLUM 3.3: Rate Limiting - 5 istek / dakika (tehlikeli işlem)
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SyncExchangeRates(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var command = new SyncExchangeRatesCommand();
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
