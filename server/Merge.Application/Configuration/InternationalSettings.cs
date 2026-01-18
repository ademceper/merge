namespace Merge.Application.Configuration;


public class InternationalSettings
{
    public const string SectionName = "InternationalSettings";

    // Currency Settings
    /// <summary>
    /// Currency code maksimum uzunluk
    /// </summary>
    public int MaxCurrencyCodeLength { get; set; } = 10;

    /// <summary>
    /// Currency name maksimum uzunluk
    /// </summary>
    public int MaxCurrencyNameLength { get; set; } = 100;

    /// <summary>
    /// Currency symbol maksimum uzunluk
    /// </summary>
    public int MaxCurrencySymbolLength { get; set; } = 10;

    /// <summary>
    /// Currency format maksimum uzunluk
    /// </summary>
    public int MaxCurrencyFormatLength { get; set; } = 50;

    /// <summary>
    /// Currency decimal places minimum değer
    /// </summary>
    public int MinCurrencyDecimalPlaces { get; set; } = 0;

    /// <summary>
    /// Currency decimal places maksimum değer
    /// </summary>
    public int MaxCurrencyDecimalPlaces { get; set; } = 10;

    /// <summary>
    /// Base currency exchange rate (her zaman 1.0 olmalı)
    /// </summary>
    public decimal BaseCurrencyExchangeRate { get; set; } = 1.0m;

    // Language Settings
    /// <summary>
    /// Language code minimum uzunluk
    /// </summary>
    public int MinLanguageCodeLength { get; set; } = 2;

    /// <summary>
    /// Language code maksimum uzunluk
    /// </summary>
    public int MaxLanguageCodeLength { get; set; } = 10;

    /// <summary>
    /// Language name maksimum uzunluk
    /// </summary>
    public int MaxLanguageNameLength { get; set; } = 100;

    /// <summary>
    /// Language native name maksimum uzunluk
    /// </summary>
    public int MaxLanguageNativeNameLength { get; set; } = 100;

    /// <summary>
    /// Language flag icon maksimum uzunluk
    /// </summary>
    public int MaxLanguageFlagIconLength { get; set; } = 500;

    // Translation Settings
    /// <summary>
    /// Translation key maksimum uzunluk
    /// </summary>
    public int MaxTranslationKeyLength { get; set; } = 200;

    /// <summary>
    /// Translation value maksimum uzunluk
    /// </summary>
    public int MaxTranslationValueLength { get; set; } = 5000;

    /// <summary>
    /// Translation category maksimum uzunluk
    /// </summary>
    public int MaxTranslationCategoryLength { get; set; } = 50;

    // Product Translation Settings
    /// <summary>
    /// Product translation name maksimum uzunluk
    /// </summary>
    public int MaxProductTranslationNameLength { get; set; } = 200;

    /// <summary>
    /// Product translation description maksimum uzunluk
    /// </summary>
    public int MaxProductTranslationDescriptionLength { get; set; } = 5000;

    /// <summary>
    /// Product translation short description maksimum uzunluk
    /// </summary>
    public int MaxProductTranslationShortDescriptionLength { get; set; } = 500;

    /// <summary>
    /// Product translation meta title maksimum uzunluk
    /// </summary>
    public int MaxProductTranslationMetaTitleLength { get; set; } = 200;

    /// <summary>
    /// Product translation meta description maksimum uzunluk
    /// </summary>
    public int MaxProductTranslationMetaDescriptionLength { get; set; } = 500;

    /// <summary>
    /// Product translation meta keywords maksimum uzunluk
    /// </summary>
    public int MaxProductTranslationMetaKeywordsLength { get; set; } = 200;

    // Category Translation Settings
    /// <summary>
    /// Category translation name maksimum uzunluk
    /// </summary>
    public int MaxCategoryTranslationNameLength { get; set; } = 200;

    /// <summary>
    /// Category translation description maksimum uzunluk
    /// </summary>
    public int MaxCategoryTranslationDescriptionLength { get; set; } = 2000;

    // International Shipping Settings
    /// <summary>
    /// Country name maksimum uzunluk
    /// </summary>
    public int MaxCountryNameLength { get; set; } = 100;

    /// <summary>
    /// City name maksimum uzunluk
    /// </summary>
    public int MaxCityNameLength { get; set; } = 100;

    /// <summary>
    /// Shipping method maksimum uzunluk
    /// </summary>
    public int MaxShippingMethodLength { get; set; } = 50;

    /// <summary>
    /// Tracking number maksimum uzunluk
    /// </summary>
    public int MaxTrackingNumberLength { get; set; } = 100;

    /// <summary>
    /// Customs declaration number maksimum uzunluk
    /// </summary>
    public int MaxCustomsDeclarationNumberLength { get; set; } = 100;

    // User Preference Settings
    /// <summary>
    /// Currency code maksimum uzunluk (user preference için)
    /// </summary>
    public int MaxUserCurrencyCodeLength { get; set; } = 10;

    /// <summary>
    /// Language code maksimum uzunluk (user preference için)
    /// </summary>
    public int MaxUserLanguageCodeLength { get; set; } = 10;
}
