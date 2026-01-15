using AutoMapper;
using Merge.Application.DTOs.International;
using Merge.Domain.SharedKernel;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Payment;
using Merge.Domain.Modules.Analytics;

namespace Merge.Application.Mappings.International;

public class InternationalMappingProfile : Profile
{
    public InternationalMappingProfile()
    {
        // International mappings
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        CreateMap<Language, LanguageDto>()
        .ConstructUsing(src => new LanguageDto(
        src.Id,
        src.Code,
        src.Name,
        src.NativeName,
        src.IsDefault,
        src.IsActive,
        src.IsRTL,
        src.FlagIcon));

        CreateMap<Currency, CurrencyDto>()
        .ConstructUsing(src => new CurrencyDto(
        src.Id,
        src.Code,
        src.Name,
        src.Symbol,
        src.ExchangeRate,
        src.IsBaseCurrency,
        src.IsActive,
        src.LastUpdated,
        src.DecimalPlaces,
        src.Format));

        CreateMap<ProductTranslation, ProductTranslationDto>()
        .ConstructUsing(src => new ProductTranslationDto(
        src.Id,
        src.ProductId,
        src.LanguageCode,
        src.Name,
        src.Description,
        src.ShortDescription,
        src.MetaTitle,
        src.MetaDescription,
        src.MetaKeywords));

        CreateMap<CategoryTranslation, CategoryTranslationDto>()
        .ConstructUsing(src => new CategoryTranslationDto(
        src.Id,
        src.CategoryId,
        src.LanguageCode,
        src.Name,
        src.Description));

        CreateMap<StaticTranslation, StaticTranslationDto>()
        .ConstructUsing(src => new StaticTranslationDto(
        src.Id,
        src.Key,
        src.LanguageCode,
        src.Value,
        src.Category));

        CreateMap<ExchangeRateHistory, ExchangeRateHistoryDto>()
        .ConstructUsing(src => new ExchangeRateHistoryDto(
        src.Id,
        src.CurrencyCode,
        src.ExchangeRate,
        src.RecordedAt,
        src.Source));


    }
}
