using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.International;
using Merge.API.Middleware;
using Merge.Application.International.Queries.GetAllLanguages;
using Merge.Application.International.Queries.GetActiveLanguages;
using Merge.Application.International.Queries.GetLanguageById;
using Merge.Application.International.Queries.GetLanguageByCode;
using Merge.Application.International.Commands.CreateLanguage;
using Merge.Application.International.Commands.UpdateLanguage;
using Merge.Application.International.Commands.DeleteLanguage;
using Merge.Application.International.Commands.CreateProductTranslation;
using Merge.Application.International.Commands.UpdateProductTranslation;
using Merge.Application.International.Commands.DeleteProductTranslation;
using Merge.Application.International.Queries.GetProductTranslations;
using Merge.Application.International.Queries.GetProductTranslation;
using Merge.Application.International.Commands.CreateCategoryTranslation;
using Merge.Application.International.Commands.UpdateCategoryTranslation;
using Merge.Application.International.Commands.DeleteCategoryTranslation;
using Merge.Application.International.Queries.GetCategoryTranslations;
using Merge.Application.International.Queries.GetStaticTranslations;
using Merge.Application.International.Commands.CreateStaticTranslation;
using Merge.Application.International.Commands.UpdateStaticTranslation;
using Merge.Application.International.Commands.DeleteStaticTranslation;
using Merge.Application.International.Commands.BulkCreateStaticTranslations;
using Merge.Application.International.Commands.SetUserLanguagePreference;
using Merge.Application.International.Queries.GetUserLanguagePreference;
using Merge.Application.International.Queries.GetTranslationStats;

namespace Merge.API.Controllers.International;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/international/languages")]
public class LanguagesController(IMediator mediator) : BaseController
{
    // Language Management
    /// <summary>
    /// Tüm dilleri getirir
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<LanguageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LanguageDto>>> GetAllLanguages(
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllLanguagesQuery();
        var languages = await mediator.Send(query, cancellationToken);
        return Ok(languages);
    }

    /// <summary>
    /// Aktif dilleri getirir
    /// </summary>
    [HttpGet("active")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<LanguageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<LanguageDto>>> GetActiveLanguages(
        CancellationToken cancellationToken = default)
    {
        var query = new GetActiveLanguagesQuery();
        var languages = await mediator.Send(query, cancellationToken);
        return Ok(languages);
    }

    /// <summary>
    /// Dil detaylarını getirir
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(LanguageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LanguageDto>> GetLanguageById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLanguageByIdQuery(id);
        var language = await mediator.Send(query, cancellationToken);

        if (language == null)
        {
            return NotFound();
        }

        return Ok(language);
    }

    /// <summary>
    /// Dil koduna göre getirir
    /// </summary>
    [HttpGet("code/{code}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(LanguageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LanguageDto>> GetLanguageByCode(
        string code,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLanguageByCodeQuery(code);
        var language = await mediator.Send(query, cancellationToken);

        if (language == null)
        {
            return NotFound();
        }

        return Ok(language);
    }

    /// <summary>
    /// Yeni dil oluşturur (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(LanguageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LanguageDto>> CreateLanguage(
        [FromBody] CreateLanguageDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateLanguageCommand(
            dto.Code,
            dto.Name,
            dto.NativeName,
            dto.IsDefault,
            dto.IsActive,
            dto.IsRTL,
            dto.FlagIcon);
        var language = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAllLanguages), new { id = language.Id }, language);
    }

    /// <summary>
    /// Dili günceller (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(LanguageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LanguageDto>> UpdateLanguage(
        Guid id,
        [FromBody] UpdateLanguageDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateLanguageCommand(
            id,
            dto.Name,
            dto.NativeName,
            dto.IsActive,
            dto.IsRTL,
            dto.FlagIcon);
        var language = await mediator.Send(command, cancellationToken);
        return Ok(language);
    }

    /// <summary>
    /// Dili siler (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteLanguage(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteLanguageCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // Product Translations
    /// <summary>
    /// Ürün çevirisi oluşturur (Admin, Seller)
    /// </summary>
    [HttpPost("products/translations")]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(ProductTranslationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductTranslationDto>> CreateProductTranslation(
        [FromBody] CreateProductTranslationDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateProductTranslationCommand(
            dto.ProductId,
            dto.LanguageCode,
            dto.Name,
            dto.Description,
            dto.ShortDescription,
            dto.MetaTitle,
            dto.MetaDescription,
            dto.MetaKeywords);
        var translation = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProductTranslation), new { productId = dto.ProductId, languageCode = dto.LanguageCode }, translation);
    }

    /// <summary>
    /// Ürün çevirilerini getirir
    /// </summary>
    [HttpGet("products/{productId}/translations")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<ProductTranslationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<ProductTranslationDto>>> GetProductTranslations(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductTranslationsQuery(productId);
        var translations = await mediator.Send(query, cancellationToken);
        return Ok(translations);
    }

    /// <summary>
    /// Belirli dil için ürün çevirisini getirir
    /// </summary>
    [HttpGet("products/{productId}/translations/{languageCode}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(ProductTranslationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductTranslationDto>> GetProductTranslation(
        Guid productId,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductTranslationQuery(productId, languageCode);
        var translation = await mediator.Send(query, cancellationToken);

        if (translation == null)
        {
            return NotFound();
        }

        return Ok(translation);
    }

    /// <summary>
    /// Ürün çevirisini günceller (Admin, Seller)
    /// </summary>
    [HttpPut("products/translations/{id}")]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(ProductTranslationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ProductTranslationDto>> UpdateProductTranslation(
        Guid id,
        [FromBody] UpdateProductTranslationDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateProductTranslationCommand(
            id,
            dto.Name,
            dto.Description,
            dto.ShortDescription,
            dto.MetaTitle,
            dto.MetaDescription,
            dto.MetaKeywords);
        var translation = await mediator.Send(command, cancellationToken);
        return Ok(translation);
    }

    /// <summary>
    /// Ürün çevirisini siler (Admin, Seller)
    /// </summary>
    [HttpDelete("products/translations/{id}")]
    [Authorize(Roles = "Admin,Seller")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteProductTranslation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteProductTranslationCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // Category Translations
    /// <summary>
    /// Kategori çevirisi oluşturur (Admin only)
    /// </summary>
    [HttpPost("categories/translations")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(CategoryTranslationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CategoryTranslationDto>> CreateCategoryTranslation(
        [FromBody] CreateCategoryTranslationDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCategoryTranslationCommand(
            dto.CategoryId,
            dto.LanguageCode,
            dto.Name,
            dto.Description);
        var translation = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetCategoryTranslations), new { categoryId = dto.CategoryId }, translation);
    }

    /// <summary>
    /// Kategori çevirilerini getirir
    /// </summary>
    [HttpGet("categories/{categoryId}/translations")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(IEnumerable<CategoryTranslationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<IEnumerable<CategoryTranslationDto>>> GetCategoryTranslations(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoryTranslationsQuery(categoryId);
        var translations = await mediator.Send(query, cancellationToken);
        return Ok(translations);
    }

    /// <summary>
    /// Kategori çevirisini günceller (Admin only)
    /// </summary>
    [HttpPut("categories/translations/{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(CategoryTranslationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<CategoryTranslationDto>> UpdateCategoryTranslation(
        Guid id,
        [FromBody] UpdateCategoryTranslationDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCategoryTranslationCommand(id, dto.Name, dto.Description);
        var translation = await mediator.Send(command, cancellationToken);
        return Ok(translation);
    }

    /// <summary>
    /// Kategori çevirisini siler (Admin only)
    /// </summary>
    [HttpDelete("categories/translations/{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteCategoryTranslation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteCategoryTranslationCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // Static Translations (UI)
    /// <summary>
    /// Statik çevirileri getirir (UI elementleri için)
    /// </summary>
    [HttpGet("translations/{languageCode}")]
    [AllowAnonymous]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    // ⚠️ NOTE: Dictionary<string, string> burada kabul edilebilir çünkü key-value çiftleri dinamik ve güvenlik riski düşük
    // Ancak gelecekte Typed DTO kullanılabilir
    public async Task<ActionResult<Dictionary<string, string>>> GetStaticTranslations(
        string languageCode,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStaticTranslationsQuery(languageCode, category);
        var translations = await mediator.Send(query, cancellationToken);
        return Ok(translations);
    }

    /// <summary>
    /// Statik çeviri oluşturur (Admin only)
    /// </summary>
    [HttpPost("translations")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(StaticTranslationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateStaticTranslation(
        [FromBody] CreateStaticTranslationDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateStaticTranslationCommand(
            dto.Key,
            dto.LanguageCode,
            dto.Value,
            dto.Category);
        var translation = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetStaticTranslations), new { languageCode = dto.LanguageCode }, translation);
    }

    /// <summary>
    /// Statik çeviriyi günceller (Admin only)
    /// </summary>
    [HttpPut("translations/{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(typeof(StaticTranslationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<StaticTranslationDto>> UpdateStaticTranslation(
        Guid id,
        [FromBody] UpdateStaticTranslationDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateStaticTranslationCommand(id, dto.Value, dto.Category);
        var translation = await mediator.Send(command, cancellationToken);
        return Ok(translation);
    }

    /// <summary>
    /// Statik çeviriyi siler (Admin only)
    /// </summary>
    [HttpDelete("translations/{id}")]
    [Authorize(Roles = "Admin")]
    [RateLimit(20, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> DeleteStaticTranslation(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var command = new DeleteStaticTranslationCommand(id);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Toplu statik çeviri oluşturur (Admin only)
    /// </summary>
    [HttpPost("translations/bulk")]
    [Authorize(Roles = "Admin")]
    [RateLimit(10, 60)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> BulkCreateStaticTranslations(
        [FromBody] BulkTranslationDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new BulkCreateStaticTranslationsCommand(dto.LanguageCode, dto.Translations);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    // User Preferences
    /// <summary>
    /// Kullanıcının dil tercihini ayarlar
    /// </summary>
    [HttpPost("preference")]
    [Authorize]
    [RateLimit(10, 60)]
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

        var command = new SetUserLanguagePreferenceCommand(userId, languageCode);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Kullanıcının dil tercihini getirir
    /// </summary>
    [HttpGet("preference")]
    [Authorize]
    [RateLimit(60, 60)]
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

        var query = new GetUserLanguagePreferenceQuery(userId);
        var languageCode = await mediator.Send(query, cancellationToken);
        return Ok(new { languageCode });
    }

    // Statistics
    /// <summary>
    /// Çeviri istatistiklerini getirir (Admin, Manager)
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    [RateLimit(60, 60)]
    [ProducesResponseType(typeof(TranslationStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<TranslationStatsDto>> GetTranslationStats(
        CancellationToken cancellationToken = default)
    {
        var query = new GetTranslationStatsQuery();
        var stats = await mediator.Send(query, cancellationToken);
        return Ok(stats);
    }
}
