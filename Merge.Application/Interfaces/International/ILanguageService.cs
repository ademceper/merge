using Merge.Application.DTOs.International;

namespace Merge.Application.Interfaces.International;

public interface ILanguageService
{
    // Language Management
    Task<IEnumerable<LanguageDto>> GetAllLanguagesAsync();
    Task<IEnumerable<LanguageDto>> GetActiveLanguagesAsync();
    Task<LanguageDto?> GetLanguageByIdAsync(Guid id);
    Task<LanguageDto?> GetLanguageByCodeAsync(string code);
    Task<LanguageDto> CreateLanguageAsync(CreateLanguageDto dto);
    Task<LanguageDto> UpdateLanguageAsync(Guid id, UpdateLanguageDto dto);
    Task DeleteLanguageAsync(Guid id);

    // Product Translations
    Task<ProductTranslationDto> CreateProductTranslationAsync(CreateProductTranslationDto dto);
    Task<ProductTranslationDto> UpdateProductTranslationAsync(Guid id, CreateProductTranslationDto dto);
    Task<IEnumerable<ProductTranslationDto>> GetProductTranslationsAsync(Guid productId);
    Task<ProductTranslationDto?> GetProductTranslationAsync(Guid productId, string languageCode);
    Task DeleteProductTranslationAsync(Guid id);

    // Category Translations
    Task<CategoryTranslationDto> CreateCategoryTranslationAsync(CreateCategoryTranslationDto dto);
    Task<CategoryTranslationDto> UpdateCategoryTranslationAsync(Guid id, CreateCategoryTranslationDto dto);
    Task<IEnumerable<CategoryTranslationDto>> GetCategoryTranslationsAsync(Guid categoryId);
    Task<CategoryTranslationDto?> GetCategoryTranslationAsync(Guid categoryId, string languageCode);
    Task DeleteCategoryTranslationAsync(Guid id);

    // Static Translations (UI elements)
    Task<StaticTranslationDto> CreateStaticTranslationAsync(CreateStaticTranslationDto dto);
    Task<StaticTranslationDto> UpdateStaticTranslationAsync(Guid id, CreateStaticTranslationDto dto);
    Task<Dictionary<string, string>> GetStaticTranslationsAsync(string languageCode, string? category = null);
    Task<string> GetStaticTranslationAsync(string key, string languageCode);
    Task DeleteStaticTranslationAsync(Guid id);
    Task BulkCreateStaticTranslationsAsync(BulkTranslationDto dto);

    // User Preferences
    Task SetUserLanguagePreferenceAsync(Guid userId, string languageCode);
    Task<string> GetUserLanguagePreferenceAsync(Guid userId);

    // Statistics
    Task<TranslationStatsDto> GetTranslationStatsAsync();
}
