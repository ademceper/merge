using AutoMapper;
using Merge.Application.DTOs.Catalog;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Search;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using System.Text.Json;

namespace Merge.Application.Mappings.Catalog;

public class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        CreateMap<ProductEntity, ProductDto>()
            .ConstructUsing(src => new ProductDto(
                src.Id,
                src.Name,
                src.Description,
                src.SKU,
                src.Price,
                src.DiscountPrice,
                src.StockQuantity,
                src.Brand,
                src.ImageUrl,
                src.ImageUrls,
                src.Rating,
                src.ReviewCount,
                src.IsActive,
                src.CategoryId,
                src.Category != null ? src.Category.Name : string.Empty,
                src.SellerId,
                src.StoreId
            ));

        CreateMap<ProductDto, ProductEntity>()
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
            .ForMember(dest => dest.CartItems, opt => opt.Ignore())
            .ForMember(dest => dest.Reviews, opt => opt.Ignore())
            .ForMember(dest => dest.Seller, opt => opt.Ignore())
            .ForMember(dest => dest.Variants, opt => opt.Ignore())
            .ForMember(dest => dest.Wishlists, opt => opt.Ignore())
            .ForMember(dest => dest.FlashSaleProducts, opt => opt.Ignore())
            .ForMember(dest => dest.BundleItems, opt => opt.Ignore())
            .ForMember(dest => dest.RecentlyViewedProducts, opt => opt.Ignore());

        CreateMap<Category, CategoryDto>()
            .ConstructUsing(src => new CategoryDto(
                src.Id,
                src.Name,
                src.Description,
                src.Slug,
                src.ImageUrl,
                src.ParentCategoryId,
                src.ParentCategory != null ? src.ParentCategory.Name : null,
                src.SubCategories != null && src.SubCategories.Any()
                    ? src.SubCategories.Select(sc => new CategoryDto(
                        sc.Id,
                        sc.Name,
                        sc.Description,
                        sc.Slug,
                        sc.ImageUrl,
                        sc.ParentCategoryId,
                        null,
                        null)).ToList().AsReadOnly()
                    : null
            ));

        CreateMap<CategoryDto, Category>()
            .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
            .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore());

        // Product → ComparisonProductDto mapping
        CreateMap<ProductEntity, ComparisonProductDto>()
            .ConstructUsing(src => new ComparisonProductDto(
                src.Id,
                src.Name,
                src.SKU,
                src.Price,
                src.DiscountPrice,
                src.ImageUrl,
                src.Brand,
                src.Category != null ? src.Category.Name : "N/A",
                src.StockQuantity,
                null,
                0,
                new Dictionary<string, string>().AsReadOnly(),
                new List<string>().AsReadOnly(),
                0
            ));

        // Search domain mappings
        CreateMap<ProductEntity, ProductSuggestionDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.DiscountPrice ?? src.Price));

        CreateMap<ProductEntity, ProductRecommendationDto>()
            .ConstructUsing(src => new ProductRecommendationDto(
                src.Id,
                src.Name,
                src.Description,
                src.Price,
                src.DiscountPrice,
                src.ImageUrl,
                src.Rating,
                src.ReviewCount,
                string.Empty,
                0
            ));

        // ProductBundle mappings
        CreateMap<ProductBundle, ProductBundleDto>()
            .ConstructUsing(src => new ProductBundleDto(
                src.Id,
                src.Name,
                src.Description,
                src.BundlePrice,
                src.OriginalTotalPrice,
                src.DiscountPercentage,
                src.ImageUrl,
                src.IsActive,
                src.StartDate,
                src.EndDate,
                src.BundleItems != null 
                    ? src.BundleItems.OrderBy(bi => bi.SortOrder).Select(bi => new BundleItemDto(
                        bi.Id,
                        bi.ProductId,
                        bi.Product != null ? bi.Product.Name : string.Empty,
                        bi.Product != null ? bi.Product.ImageUrl : string.Empty,
                        bi.Product != null ? (bi.Product.DiscountPrice ?? bi.Product.Price) : 0,
                        bi.Quantity,
                        bi.SortOrder
                    )).ToList().AsReadOnly()
                    : Array.Empty<BundleItemDto>().AsReadOnly()
            ));
        CreateMap<CreateProductBundleDto, ProductBundle>();
        CreateMap<UpdateProductBundleDto, ProductBundle>();

        CreateMap<BundleItem, BundleItemDto>()
            .ConstructUsing(src => new BundleItemDto(
                src.Id,
                src.ProductId,
                src.Product != null ? src.Product.Name : string.Empty,
                src.Product != null ? src.Product.ImageUrl : string.Empty,
                src.Product != null ? (src.Product.DiscountPrice ?? src.Product.Price) : 0,
                src.Quantity,
                src.SortOrder
            ));

        // ProductQuestion mappings
        CreateMap<ProductQuestion, ProductQuestionDto>()
            .ConstructUsing(src => new ProductQuestionDto(
                src.Id,
                src.ProductId,
                src.Product != null ? src.Product.Name : string.Empty,
                src.UserId,
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty,
                src.Question,
                src.IsApproved,
                src.AnswerCount,
                src.HelpfulCount,
                src.HasSellerAnswer,
                false,
                src.CreatedAt,
                src.Answers != null 
                    ? src.Answers.Where(a => a.IsApproved).Select(a => new ProductAnswerDto(
                        a.Id,
                        a.QuestionId,
                        a.UserId,
                        a.User != null ? $"{a.User.FirstName} {a.User.LastName}" : string.Empty,
                        a.Answer,
                        a.IsApproved,
                        a.IsSellerAnswer,
                        a.IsVerifiedPurchase,
                        a.HelpfulCount,
                        false,
                        a.CreatedAt
                    )).ToList().AsReadOnly()
                    : Array.Empty<ProductAnswerDto>().AsReadOnly()
            ));
        CreateMap<CreateProductQuestionDto, ProductQuestion>();

        // ProductAnswer mappings
        CreateMap<ProductAnswer, ProductAnswerDto>()
            .ConstructUsing(src => new ProductAnswerDto(
                src.Id,
                src.QuestionId,
                src.UserId,
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty,
                src.Answer,
                src.IsApproved,
                src.IsSellerAnswer,
                src.IsVerifiedPurchase,
                src.HelpfulCount,
                false,
                src.CreatedAt
            ));
        CreateMap<CreateProductAnswerDto, ProductAnswer>();

        // ProductTemplate mappings
        CreateMap<ProductTemplate, ProductTemplateDto>()
            .ConvertUsing((src, context) => new ProductTemplateDto(
                src.Id,
                src.Name,
                src.Description,
                src.CategoryId,
                src.Category != null ? src.Category.Name : string.Empty,
                src.Brand,
                src.DefaultSKUPrefix,
                src.DefaultPrice,
                src.DefaultStockQuantity,
                src.DefaultImageUrl,
                !string.IsNullOrEmpty(src.Specifications)
                    ? JsonSerializer.Deserialize<IReadOnlyDictionary<string, string>>(src.Specifications)
                    : null,
                !string.IsNullOrEmpty(src.Attributes)
                    ? JsonSerializer.Deserialize<IReadOnlyDictionary<string, string>>(src.Attributes)
                    : null,
                src.IsActive,
                src.UsageCount,
                src.CreatedAt
            ));
        CreateMap<CreateProductTemplateDto, ProductTemplate>();
        CreateMap<UpdateProductTemplateDto, ProductTemplate>();

        // SizeGuide mappings
        CreateMap<SizeGuide, SizeGuideDto>()
            .ConvertUsing((src, context) => new SizeGuideDto(
                src.Id,
                src.Name,
                src.Description,
                src.CategoryId,
                src.Category != null ? src.Category.Name : string.Empty,
                src.Brand,
                src.Type.ToString(),
                src.MeasurementUnit,
                src.IsActive,
                src.Entries != null 
                    ? src.Entries.OrderBy(e => e.DisplayOrder).Select(e => new SizeGuideEntryDto(
                        e.Id,
                        e.SizeLabel,
                        e.AlternativeLabel,
                        e.Chest,
                        e.Waist,
                        e.Hips,
                        e.Inseam,
                        e.Shoulder,
                        e.Length,
                        e.Width,
                        e.Height,
                        e.Weight,
                        !string.IsNullOrEmpty(e.AdditionalMeasurements)
                            ? JsonSerializer.Deserialize<IReadOnlyDictionary<string, string>>(e.AdditionalMeasurements)
                            : null,
                        e.DisplayOrder
                    )).ToList().AsReadOnly()
                    : Array.Empty<SizeGuideEntryDto>().AsReadOnly()
            ));

        CreateMap<SizeGuideEntry, SizeGuideEntryDto>()
            .ConvertUsing((src, context) => new SizeGuideEntryDto(
                src.Id,
                src.SizeLabel,
                src.AlternativeLabel,
                src.Chest,
                src.Waist,
                src.Hips,
                src.Inseam,
                src.Shoulder,
                src.Length,
                src.Width,
                src.Height,
                src.Weight,
                !string.IsNullOrEmpty(src.AdditionalMeasurements)
                    ? JsonSerializer.Deserialize<IReadOnlyDictionary<string, string>>(src.AdditionalMeasurements)
                    : null,
                src.DisplayOrder
            ));

        // ProductComparison mappings
        CreateMap<ProductComparison, ProductComparisonDto>()
            .ConstructUsing(src => new ProductComparisonDto(
                src.Id,
                src.UserId,
                src.Name,
                src.IsSaved,
                src.ShareCode,
                Array.Empty<ComparisonProductDto>().AsReadOnly(),
                src.CreatedAt
            ));
    }
}
