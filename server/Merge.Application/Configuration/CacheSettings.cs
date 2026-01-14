namespace Merge.Application.Configuration;

/// <summary>
/// Cache expiration settings - BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (Clean Architecture)
/// </summary>
public class CacheSettings
{
    public const string SectionName = "CacheSettings";

    // ✅ BOLUM 12.0: Magic Number'ları Constants'a Taşıma (Clean Architecture)
    // Product-related cache expiration times (in minutes)
    public int ProductCacheExpirationMinutes { get; set; } = 15; // Products change more frequently
    public int ProductSearchCacheExpirationMinutes { get; set; } = 5; // Search results can be dynamic
    public int ProductCategoryCacheExpirationMinutes { get; set; } = 15; // Products change more frequently
    
    // Question/Answer-related cache expiration times (in minutes)
    public int QuestionCacheExpirationMinutes { get; set; } = 15; // Questions change more frequently
    public int AnswerCacheExpirationMinutes { get; set; } = 15; // Answers change more frequently
    public int ProductQuestionsCacheExpirationMinutes { get; set; } = 5; // Product questions can change frequently
    public int UserQuestionsCacheExpirationMinutes { get; set; } = 5; // User questions can change frequently
    public int UnansweredQuestionsCacheExpirationMinutes { get; set; } = 5; // Unanswered questions change frequently
    public int QAStatsCacheExpirationMinutes { get; set; } = 10; // Stats change frequently
    
    // Template-related cache expiration times (in minutes)
    public int ProductTemplateCacheExpirationMinutes { get; set; } = 30; // Templates change less frequently
    public int PopularTemplatesCacheExpirationMinutes { get; set; } = 30; // Templates change less frequently
    
    // Bundle-related cache expiration times (in minutes)
    public int ProductBundleCacheExpirationMinutes { get; set; } = 15; // Bundles change more frequently
    
    // Comparison-related cache expiration times (in minutes)
    public int ProductComparisonCacheExpirationMinutes { get; set; } = 10; // Comparisons can change
    public int ComparisonMatrixCacheExpirationMinutes { get; set; } = 5; // Matrix can change when products are updated
    public int UserComparisonCacheExpirationMinutes { get; set; } = 5; // Current comparison can change frequently
    public int SharedComparisonCacheExpirationMinutes { get; set; } = 10; // Shared comparisons can change
    
    // Size Guide-related cache expiration times (in minutes)
    public int SizeGuideCacheExpirationMinutes { get; set; } = 30; // Size guides change less frequently
    public int SizeRecommendationCacheExpirationMinutes { get; set; } = 30; // Recommendations change less frequently
    public int ProductSizeGuideCacheExpirationMinutes { get; set; } = 30; // Size guides change less frequently
    public int NoSizeGuideCacheExpirationMinutes { get; set; } = 5; // No guide result cache (short TTL)
}
