using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.International;
using Merge.Application.DTOs.International;
using Merge.API.Middleware;

namespace Merge.API.Controllers.International;

[ApiController]
[Route("api/international/languages")]
public class LanguagesController : BaseController
{
    private readonly ILanguageService _languageService;

    public LanguagesController(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    // Language Management
    /// <summary>
    /// Tüm dilleri getirir
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<LanguageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LanguageDto>>> GetAllLanguages(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var languages = await _languageService.GetAllLanguagesAsync(cancellationToken);
        return Ok(languages);
    }

    /// <summary>
    /// Aktif dilleri getirir
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<LanguageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LanguageDto>>> GetActiveLanguages(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var languages = await _languageService.GetActiveLanguagesAsync(cancellationToken);
        return Ok(languages);
    }

    /// <summary>
    /// Yeni dil oluşturur (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(typeof(LanguageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LanguageDto>> CreateLanguage(
        [FromBody] CreateLanguageDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var language = await _languageService.CreateLanguageAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetAllLanguages), new { id = language.Id }, language);
    }

    // Product Translations
    /// <summary>
    /// Ürün çevirisi oluşturur (Admin, Seller)
    /// </summary>
    [HttpPost("products/translations")]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(ProductTranslationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductTranslationDto>> CreateProductTranslation(
        [FromBody] CreateProductTranslationDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var translation = await _languageService.CreateProductTranslationAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetProductTranslation), new { productId = dto.ProductId, languageCode = dto.LanguageCode }, translation);
    }

    /// <summary>
    /// Ürün çevirilerini getirir
    /// </summary>
    [HttpGet("products/{productId}/translations")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<ProductTranslationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductTranslationDto>>> GetProductTranslations(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var translations = await _languageService.GetProductTranslationsAsync(productId, cancellationToken);
        return Ok(translations);
    }

    /// <summary>
    /// Belirli dil için ürün çevirisini getirir
    /// </summary>
    [HttpGet("products/{productId}/translations/{languageCode}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(ProductTranslationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductTranslationDto>> GetProductTranslation(
        Guid productId,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var translation = await _languageService.GetProductTranslationAsync(productId, languageCode, cancellationToken);

        if (translation == null)
        {
            return NotFound();
        }

        return Ok(translation);
    }

    // Category Translations
    /// <summary>
    /// Kategori çevirisi oluşturur (Admin only)
    /// </summary>
    [HttpPost("categories/translations")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(typeof(CategoryTranslationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CategoryTranslationDto>> CreateCategoryTranslation(
        [FromBody] CreateCategoryTranslationDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var translation = await _languageService.CreateCategoryTranslationAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetCategoryTranslations), new { categoryId = dto.CategoryId }, translation);
    }

    /// <summary>
    /// Kategori çevirilerini getirir
    /// </summary>
    [HttpGet("categories/{categoryId}/translations")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(IEnumerable<CategoryTranslationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<CategoryTranslationDto>>> GetCategoryTranslations(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var translations = await _languageService.GetCategoryTranslationsAsync(categoryId, cancellationToken);
        return Ok(translations);
    }

    // Static Translations (UI)
    /// <summary>
    /// Statik çevirileri getirir (UI elementleri için)
    /// </summary>
    [HttpGet("translations/{languageCode}")]
    [AllowAnonymous]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    // ⚠️ NOTE: Dictionary<string, string> burada kabul edilebilir çünkü key-value çiftleri dinamik ve güvenlik riski düşük
    // Ancak gelecekte Typed DTO kullanılabilir
    public async Task<ActionResult<Dictionary<string, string>>> GetStaticTranslations(
        string languageCode,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var translations = await _languageService.GetStaticTranslationsAsync(languageCode, category, cancellationToken);
        return Ok(translations);
    }

    /// <summary>
    /// Statik çeviri oluşturur (Admin only)
    /// </summary>
    [HttpPost("translations")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)] // ✅ BOLUM 3.3: Rate Limiting - 20 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateStaticTranslation(
        [FromBody] CreateStaticTranslationDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _languageService.CreateStaticTranslationAsync(dto, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Toplu statik çeviri oluşturur (Admin only)
    /// </summary>
    [HttpPost("translations/bulk")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> BulkCreateStaticTranslations(
        [FromBody] BulkTranslationDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _languageService.BulkCreateStaticTranslationsAsync(dto, cancellationToken);
        return NoContent();
    }

    // User Preferences
    /// <summary>
    /// Kullanıcının dil tercihini ayarlar
    /// </summary>
    [HttpPost("preference")]
    [Authorize]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10 istek / dakika
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> SetLanguagePreference(
        [FromBody] string languageCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return BadRequest("Dil kodu boş olamaz.");
        }

        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi tercihini ayarlayabilir
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        await _languageService.SetUserLanguagePreferenceAsync(userId, languageCode, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Kullanıcının dil tercihini getirir
    /// </summary>
    [HttpGet("preference")]
    [Authorize]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<string>> GetLanguagePreference(
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi tercihini görebilir
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var languageCode = await _languageService.GetUserLanguagePreferenceAsync(userId, cancellationToken);
        return Ok(new { languageCode });
    }

    // Statistics
    /// <summary>
    /// Çeviri istatistiklerini getirir (Admin, Manager)
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(TranslationStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TranslationStatsDto>> GetTranslationStats(
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var stats = await _languageService.GetTranslationStatsAsync(cancellationToken);
        return Ok(stats);
    }
}
