using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.DTOs.Logistics;
using Merge.Application.DTOs.Marketing;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Search;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Payment;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Catalog;
using System.Text.Json;

namespace Merge.Application.Mappings.Payment;

public class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<Merge.Domain.Modules.Payment.Payment, PaymentDto>()
        .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : string.Empty));
        CreateMap<CreatePaymentDto, Merge.Domain.Modules.Payment.Payment>();

        CreateMap<Shipping, ShippingDto>()
        .ConstructUsing(src => new ShippingDto(
        src.Id,
        src.OrderId,
        src.Order != null ? src.Order.OrderNumber : string.Empty,
        src.ShippingProvider,
        src.TrackingNumber,
        src.Status, // ✅ Enum direkt kullanılıyor (ToString() YASAK)
        src.ShippedDate,
        src.EstimatedDeliveryDate,
        src.DeliveredDate,
        src.ShippingCost,
        src.ShippingLabelUrl,
        src.CreatedAt
        ));
        CreateMap<CreateShippingDto, Shipping>();

        CreateMap<CreateAddressDto, Merge.Domain.Modules.Identity.Address>();
        CreateMap<UpdateAddressDto, Merge.Domain.Modules.Identity.Address>();

        // FlashSale mappings
        CreateMap<FlashSale, FlashSaleDto>()
        .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.FlashSaleProducts));
        CreateMap<CreateFlashSaleDto, FlashSale>();
        CreateMap<UpdateFlashSaleDto, FlashSale>();

        CreateMap<FlashSaleProduct, FlashSaleProductDto>()
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
        .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : string.Empty))
        .ForMember(dest => dest.OriginalPrice, opt => opt.MapFrom(src => src.Product != null ? src.Product.Price : 0))
        .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.StockLimit - src.SoldQuantity))
        .ForMember(dest => dest.DiscountPercentage, opt => opt.MapFrom(src => 
        src.Product != null && src.Product.Price > 0 
        ? ((src.Product.Price - src.SalePrice) / src.Product.Price) * 100 
        : 0));

        // ProductBundle mappings
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

        // BundleItem mappings
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
        false, // HasUserVoted - Set manually in handler with 'with' expression
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
        false, // HasUserVoted - Set manually in handler
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
        false, // HasUserVoted - Set manually in handler with 'with' expression
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

        // SizeGuideEntry mappings
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
        Array.Empty<ComparisonProductDto>().AsReadOnly(), // Products - Set manually in handler
        src.CreatedAt
        ));

        // Product → ComparisonProductDto mapping
        CreateMap<Merge.Domain.Modules.Catalog.Product, ComparisonProductDto>()
        .ConstructUsing(src => new ComparisonProductDto(
        src.Id, // ProductId
        src.Name,
        src.SKU,
        src.Price,
        src.DiscountPrice,
        src.ImageUrl, // MainImage
        src.Brand,
        src.Category != null ? src.Category.Name : "N/A",
        src.StockQuantity,
        null, // Rating - Set manually in handler with 'with' expression
        0, // ReviewCount - Set manually in handler with 'with' expression
        new Dictionary<string, string>().AsReadOnly(), // Specifications - Set manually in handler
        new List<string>().AsReadOnly(), // Features - Set manually in handler
        0 // Position - Set manually in handler with 'with' expression
        ));

        // Search domain mappings
        // Product → ProductSuggestionDto mapping
        CreateMap<Merge.Domain.Modules.Catalog.Product, ProductSuggestionDto>()
        .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
        .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.DiscountPrice ?? src.Price));

        // Product → ProductRecommendationDto mapping
        CreateMap<Merge.Domain.Modules.Catalog.Product, ProductRecommendationDto>()
        .ConstructUsing(src => new ProductRecommendationDto(
        src.Id, // ProductId
        src.Name,
        src.Description,
        src.Price,
        src.DiscountPrice,
        src.ImageUrl,
        src.Rating,
        src.ReviewCount,
        string.Empty, // RecommendationReason - Set manually in handler with 'with' expression
        0 // RecommendationScore - Set manually in handler with 'with' expression
        ));

        // Invoice mappings
        CreateMap<Invoice, InvoiceDto>()
        .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : string.Empty))
        .ForMember(dest => dest.BillingAddress, opt => opt.MapFrom(src => src.Order != null ? src.Order.Address : null))
        .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Order != null && src.Order.OrderItems != null 
        ? src.Order.OrderItems.ToList() 
        : new List<OrderItem>()))
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        // PaymentMethod mappings
        CreateMap<PaymentMethod, PaymentMethodDto>()
        .AfterMap((src, dest) =>
        {
        dest.Settings = !string.IsNullOrEmpty(src.Settings)
        ? JsonSerializer.Deserialize<PaymentMethodSettingsDto>(src.Settings)
        : null;
        });

    }
}
