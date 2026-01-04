using Merge.Application.DTOs.International;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.International;

public interface ILanguageService
{
    // Language Management
    Task<IEnumerable<LanguageDto>> GetAllLanguagesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<LanguageDto>> GetActiveLanguagesAsync(CancellationToken cancellationToken = default);
    Task<LanguageDto?> GetLanguageByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LanguageDto?> GetLanguageByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<LanguageDto> CreateLanguageAsync(CreateLanguageDto dto, CancellationToken cancellationToken = default);
    Task<LanguageDto> UpdateLanguageAsync(Guid id, UpdateLanguageDto dto, CancellationToken cancellationToken = default);
    Task DeleteLanguageAsync(Guid id, CancellationToken cancellationToken = default);

    // Product Translations
    Task<ProductTranslationDto> CreateProductTranslationAsync(CreateProductTranslationDto dto, CancellationToken cancellationToken = default);
    Task<ProductTranslationDto> UpdateProductTranslationAsync(Guid id, CreateProductTranslationDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductTranslationDto>> GetProductTranslationsAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductTranslationDto?> GetProductTranslationAsync(Guid productId, string languageCode, CancellationToken cancellationToken = default);
    Task DeleteProductTranslationAsync(Guid id, CancellationToken cancellationToken = default);

    // Category Translations
    Task<CategoryTranslationDto> CreateCategoryTranslationAsync(CreateCategoryTranslationDto dto, CancellationToken cancellationToken = default);
    Task<CategoryTranslationDto> UpdateCategoryTranslationAsync(Guid id, CreateCategoryTranslationDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryTranslationDto>> GetCategoryTranslationsAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<CategoryTranslationDto?> GetCategoryTranslationAsync(Guid categoryId, string languageCode, CancellationToken cancellationToken = default);
    Task DeleteCategoryTranslationAsync(Guid id, CancellationToken cancellationToken = default);

    // Static Translations (UI elements)
    Task<StaticTranslationDto> CreateStaticTranslationAsync(CreateStaticTranslationDto dto, CancellationToken cancellationToken = default);
    Task<StaticTranslationDto> UpdateStaticTranslationAsync(Guid id, CreateStaticTranslationDto dto, CancellationToken cancellationToken = default);
    // ⚠️ NOTE: Dictionary<string, string> burada kabul edilebilir çünkü key-value çiftleri dinamik ve güvenlik riski düşük
    Task<Dictionary<string, string>> GetStaticTranslationsAsync(string languageCode, string? category = null, CancellationToken cancellationToken = default);
    Task<string> GetStaticTranslationAsync(string key, string languageCode, CancellationToken cancellationToken = default);
    Task DeleteStaticTranslationAsync(Guid id, CancellationToken cancellationToken = default);
    Task BulkCreateStaticTranslationsAsync(BulkTranslationDto dto, CancellationToken cancellationToken = default);

    // User Preferences
    Task SetUserLanguagePreferenceAsync(Guid userId, string languageCode, CancellationToken cancellationToken = default);
    Task<string> GetUserLanguagePreferenceAsync(Guid userId, CancellationToken cancellationToken = default);

    // Statistics
    Task<TranslationStatsDto> GetTranslationStatsAsync(CancellationToken cancellationToken = default);
}
