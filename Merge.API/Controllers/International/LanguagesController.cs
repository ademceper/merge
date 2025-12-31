using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.International;
using Merge.Application.DTOs.International;


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
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LanguageDto>>> GetAllLanguages()
    {
        var languages = await _languageService.GetAllLanguagesAsync();
        return Ok(languages);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<LanguageDto>>> GetActiveLanguages()
    {
        var languages = await _languageService.GetActiveLanguagesAsync();
        return Ok(languages);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LanguageDto>> CreateLanguage([FromBody] CreateLanguageDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var language = await _languageService.CreateLanguageAsync(dto);
        return CreatedAtAction(nameof(GetAllLanguages), new { id = language.Id }, language);
    }

    // Product Translations
    [HttpPost("products/translations")]
    [Authorize(Roles = "Admin,Seller")]
    public async Task<ActionResult<ProductTranslationDto>> CreateProductTranslation([FromBody] CreateProductTranslationDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var translation = await _languageService.CreateProductTranslationAsync(dto);
        return CreatedAtAction(nameof(GetProductTranslation), new { productId = dto.ProductId, languageCode = dto.LanguageCode }, translation);
    }

    [HttpGet("products/{productId}/translations")]
    public async Task<ActionResult<IEnumerable<ProductTranslationDto>>> GetProductTranslations(Guid productId)
    {
        var translations = await _languageService.GetProductTranslationsAsync(productId);
        return Ok(translations);
    }

    [HttpGet("products/{productId}/translations/{languageCode}")]
    public async Task<ActionResult<ProductTranslationDto>> GetProductTranslation(Guid productId, string languageCode)
    {
        var translation = await _languageService.GetProductTranslationAsync(productId, languageCode);

        if (translation == null)
        {
            return NotFound();
        }

        return Ok(translation);
    }

    // Category Translations
    [HttpPost("categories/translations")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryTranslationDto>> CreateCategoryTranslation([FromBody] CreateCategoryTranslationDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        var translation = await _languageService.CreateCategoryTranslationAsync(dto);
        return CreatedAtAction(nameof(GetCategoryTranslations), new { categoryId = dto.CategoryId }, translation);
    }

    [HttpGet("categories/{categoryId}/translations")]
    public async Task<ActionResult<IEnumerable<CategoryTranslationDto>>> GetCategoryTranslations(Guid categoryId)
    {
        var translations = await _languageService.GetCategoryTranslationsAsync(categoryId);
        return Ok(translations);
    }

    // Static Translations (UI)
    [HttpGet("translations/{languageCode}")]
    public async Task<ActionResult<Dictionary<string, string>>> GetStaticTranslations(
        string languageCode,
        [FromQuery] string? category = null)
    {
        var translations = await _languageService.GetStaticTranslationsAsync(languageCode, category);
        return Ok(translations);
    }

    [HttpPost("translations")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStaticTranslation([FromBody] CreateStaticTranslationDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        await _languageService.CreateStaticTranslationAsync(dto);
        return NoContent();
    }

    [HttpPost("translations/bulk")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> BulkCreateStaticTranslations([FromBody] BulkTranslationDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null) return validationResult;

        await _languageService.BulkCreateStaticTranslationsAsync(dto);
        return NoContent();
    }

    // User Preferences
    [HttpPost("preference")]
    [Authorize]
    public async Task<IActionResult> SetLanguagePreference([FromBody] string languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return BadRequest("Dil kodu boş olamaz.");
        }

        var userId = GetUserId();
        await _languageService.SetUserLanguagePreferenceAsync(userId, languageCode);
        return NoContent();
    }

    [HttpGet("preference")]
    [Authorize]
    public async Task<ActionResult<string>> GetLanguagePreference()
    {
        var userId = GetUserId();
        var languageCode = await _languageService.GetUserLanguagePreferenceAsync(userId);
        return Ok(new { languageCode });
    }

    // Statistics
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<TranslationStatsDto>> GetTranslationStats()
    {
        var stats = await _languageService.GetTranslationStatsAsync();
        return Ok(stats);
    }
}
