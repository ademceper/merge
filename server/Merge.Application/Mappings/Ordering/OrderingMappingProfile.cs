using AutoMapper;
using Merge.Application.DTOs.Cart;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Catalog;
using Merge.Application.DTOs.Marketing;
using Merge.Application.DTOs.Notification;
using Merge.Application.DTOs.Review;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Payment;
using Merge.Application.DTOs.Logistics;
using Merge.Application.DTOs.User;
using Merge.Application.DTOs.Search;
using Merge.Application.DTOs.Support;
using Merge.Application.DTOs.Content;
using Merge.Application.DTOs.Seller;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.B2B;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Support;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Payment;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Analytics;
using InventoryEntity = Merge.Domain.Modules.Inventory.Inventory;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.ValueObjects;
using System.Text.Json;

namespace Merge.Application.Mappings.Ordering;

public class OrderingMappingProfile : Profile
{
    public OrderingMappingProfile()
    {
        // ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı) - Positional constructor kullanımı
        CreateMap<CartItem, CartItemDto>()
        .ConstructUsing(src => new CartItemDto(
        src.Id,
        src.ProductId,
        src.ProductVariantId,
        src.Product != null ? src.Product.Name : string.Empty,
        src.Product != null ? src.Product.ImageUrl : string.Empty,
        src.Quantity,
        src.Price,
        src.Quantity * src.Price
        ));

        // ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı) - Positional constructor kullanımı
        // ✅ BOLUM 7.1.9: Collection Expressions (C# 12) - ToList().AsReadOnly() yerine direkt IReadOnlyList
        CreateMap<Merge.Domain.Modules.Ordering.Cart, CartDto>()
        .ConstructUsing(src => new CartDto(
        src.Id,
        src.UserId,
        src.CartItems.Select(ci => new CartItemDto(
        ci.Id,
        ci.ProductId,
        ci.ProductVariantId,
        ci.Product != null ? ci.Product.Name : string.Empty,
        ci.Product != null ? ci.Product.ImageUrl : string.Empty,
        ci.Quantity,
        ci.Price,
        ci.Quantity * ci.Price
        )).ToArray(), // ✅ BOLUM 7.1.9: Collection Expressions - Array kullanımı (IReadOnlyList'e otomatik cast)
        src.CalculateTotalAmount()
        ));

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        CreateMap<SavedCartItem, SavedCartItemDto>()
        .ConstructUsing(src => new SavedCartItemDto(
        src.Id,
        src.ProductId,
        src.Product != null ? src.Product.Name : string.Empty,
        src.Product != null ? src.Product.ImageUrl : string.Empty,
        src.Price,
        src.Product != null ? (src.Product.DiscountPrice ?? src.Product.Price) : 0,
        src.Quantity,
        src.Notes,
        src.Product != null && src.Price != (src.Product.DiscountPrice ?? src.Product.Price)
        ));

        CreateMap<OrderItem, OrderItemDto>()
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
        .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : string.Empty))
        .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.UnitPrice))
        .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice));

        CreateMap<Merge.Domain.Modules.Ordering.Order, OrderDto>()
        .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

        CreateMap<Wishlist, ProductDto>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Product.Id))
        .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product.Name))
        .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Product.Description))
        .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
        .ForMember(dest => dest.DiscountPrice, opt => opt.MapFrom(src => src.Product.DiscountPrice))
        .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl))
        .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.Product.Rating))
        .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.Product.ReviewCount))
        .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Product.CategoryId))
        .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Product.Category.Name));

        CreateMap<Coupon, CouponDto>();
        CreateMap<CouponDto, Coupon>();

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // ✅ BOLUM 1.2: Enum kullanımı (string Type YASAK)
        // ✅ FIX: Notification namespace conflict - using alias
        CreateMap<Merge.Domain.Modules.Notifications.Notification, NotificationDto>()
        .ConstructUsing(src => new NotificationDto(
        src.Id,
        src.UserId,
        src.Type,
        src.Title,
        src.Message,
        src.IsRead,
        src.ReadAt,
        src.Link,
        src.Data,
        src.CreatedAt));

        // CreateNotificationDto artık command'da kullanılıyor, mapping gerekmiyor

        CreateMap<ReviewEntity, ReviewDto>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));
        CreateMap<CreateReviewDto, ReviewEntity>();
        CreateMap<UpdateReviewDto, ReviewEntity>();

        // TrustBadge mappings
        CreateMap<TrustBadge, TrustBadgeDto>()
        .AfterMap((src, dest) =>
        {
        // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
        // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
        dest.Criteria = !string.IsNullOrEmpty(src.Criteria)
        ? JsonSerializer.Deserialize<TrustBadgeSettingsDto>(src.Criteria)
        : null;
        });
        CreateMap<CreateTrustBadgeDto, TrustBadge>();
        CreateMap<UpdateTrustBadgeDto, TrustBadge>();

        // SellerTrustBadge mappings
        CreateMap<SellerTrustBadge, SellerTrustBadgeDto>()
        .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller != null ? src.Seller.StoreName : string.Empty));

        // ProductTrustBadge mappings
        CreateMap<ProductTrustBadge, ProductTrustBadgeDto>()
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

        // ReviewHelpfulnessStatsDto - Review entity'den mapping (computed properties)
        CreateMap<ReviewEntity, ReviewHelpfulnessStatsDto>()
        .ForMember(dest => dest.ReviewId, opt => opt.MapFrom(src => src.Id))
        .ForMember(dest => dest.TotalVotes, opt => opt.MapFrom(src => src.HelpfulCount + src.UnhelpfulCount))
        .ForMember(dest => dest.HelpfulPercentage, opt => opt.MapFrom(src => 
        (src.HelpfulCount + src.UnhelpfulCount) > 0 
        ? Math.Round((decimal)src.HelpfulCount / (src.HelpfulCount + src.UnhelpfulCount) * 100, 2) 
        : 0))
        .ForMember(dest => dest.UserVote, opt => opt.Ignore()); // Set manually in service

        CreateMap<ReturnRequest, ReturnRequestDto>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
        .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : string.Empty));
        CreateMap<CreateReturnRequestDto, ReturnRequest>();

        CreateMap<Merge.Domain.Modules.Payment.Payment, PaymentDto>()
        .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : string.Empty));
        CreateMap<CreatePaymentDto, Merge.Domain.Modules.Payment.Payment>();

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
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
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
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
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
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
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
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
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
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
        // ✅ BOLUM 7.1.5: Records - ConvertUsing ile record mapping (expression tree limitation için)
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
        // ✅ BOLUM 7.1.5: Records - ConvertUsing ile record mapping (expression tree limitation için)
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
        // ✅ BOLUM 7.1.5: Records - ConvertUsing ile record mapping (expression tree limitation için)
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
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
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
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
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
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
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
        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK) - Status zaten enum, direkt map edilebilir
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        // PaymentMethod mappings
        CreateMap<PaymentMethod, PaymentMethodDto>()
        .AfterMap((src, dest) =>
        {
        // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
        // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
        dest.Settings = !string.IsNullOrEmpty(src.Settings)
        ? JsonSerializer.Deserialize<PaymentMethodSettingsDto>(src.Settings)
        : null;
        });

        // FAQ mappings
        // ✅ BOLUM 7.1.5: Records - ConvertUsing ile record mapping
        CreateMap<FAQ, FaqDto>()
        .ConvertUsing(src => new FaqDto(
        src.Id,
        src.Question,
        src.Answer,
        src.Category,
        src.SortOrder,
        src.ViewCount,
        src.IsPublished,
        null // Links will be set in controller
        ));

        CreateMap<FaqDto, FAQ>()
        .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.RowVersion, opt => opt.Ignore());

        CreateMap<CreateFaqDto, FAQ>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.RowVersion, opt => opt.Ignore());

        CreateMap<UpdateFaqDto, FAQ>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.ViewCount, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.RowVersion, opt => opt.Ignore());

        // Banner mappings
        CreateMap<Banner, BannerDto>().ReverseMap();
        CreateMap<CreateBannerDto, Banner>();
        CreateMap<UpdateBannerDto, Banner>();

        // GiftCard mappings
        CreateMap<GiftCard, GiftCardDto>().ReverseMap();

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // Warehouse mappings
        CreateMap<Warehouse, WarehouseDto>()
        .ConstructUsing(src => new WarehouseDto(
        src.Id,
        src.Name,
        src.Code,
        src.Address,
        src.City,
        src.Country,
        src.PostalCode,
        src.ContactPerson,
        src.ContactPhone,
        src.ContactEmail,
        src.Capacity,
        src.IsActive,
        src.Description,
        src.CreatedAt
        ));
        CreateMap<CreateWarehouseDto, Warehouse>();
        CreateMap<UpdateWarehouseDto, Warehouse>();

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // Inventory mappings
        CreateMap<InventoryEntity, InventoryDto>()
        .ConstructUsing(src => new InventoryDto(
        src.Id,
        src.ProductId,
        src.Product != null ? src.Product.Name : string.Empty,
        src.Product != null ? src.Product.SKU : string.Empty,
        src.WarehouseId,
        src.Warehouse != null ? src.Warehouse.Name : string.Empty,
        src.Warehouse != null ? src.Warehouse.Code : string.Empty,
        src.Quantity,
        src.ReservedQuantity,
        src.AvailableQuantity,
        src.MinimumStockLevel,
        src.MaximumStockLevel,
        src.UnitCost,
        src.Location,
        src.LastRestockedAt,
        src.LastCountedAt,
        src.CreatedAt
        ));
        CreateMap<CreateInventoryDto, InventoryEntity>();
        CreateMap<UpdateInventoryDto, InventoryEntity>();

        // LowStockAlert mappings
        CreateMap<InventoryEntity, LowStockAlertDto>()
        .ConstructUsing(src => new LowStockAlertDto(
        src.ProductId,
        src.Product != null ? src.Product.Name : string.Empty,
        src.Product != null ? src.Product.SKU : string.Empty,
        src.WarehouseId,
        src.Warehouse != null ? src.Warehouse.Name : string.Empty,
        src.Quantity,
        src.MinimumStockLevel,
        src.MinimumStockLevel - src.Quantity
        ));

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // StockMovement mappings
        CreateMap<StockMovement, StockMovementDto>()
        .ConstructUsing(src => new StockMovementDto(
        src.Id,
        src.InventoryId,
        src.ProductId,
        src.Product != null ? src.Product.Name : string.Empty,
        src.Product != null ? src.Product.SKU : string.Empty,
        src.WarehouseId,
        src.Warehouse != null ? src.Warehouse.Name : string.Empty,
        src.MovementType,
        src.MovementType.ToString(),
        src.Quantity,
        src.QuantityBefore,
        src.QuantityAfter,
        src.ReferenceNumber,
        src.ReferenceId,
        src.Notes,
        src.PerformedBy,
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null,
        src.FromWarehouseId,
        src.FromWarehouse != null ? src.FromWarehouse.Name : null,
        src.ToWarehouseId,
        src.ToWarehouse != null ? src.ToWarehouse.Name : null,
        src.CreatedAt
        ));
        CreateMap<CreateStockMovementDto, StockMovement>();

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // DeliveryTimeEstimation mappings
        CreateMap<DeliveryTimeEstimation, DeliveryTimeEstimationDto>()
        .ConstructUsing(src => new DeliveryTimeEstimationDto(
        src.Id,
        src.ProductId,
        src.Product != null ? src.Product.Name : null,
        src.CategoryId,
        src.Category != null ? src.Category.Name : null,
        src.WarehouseId,
        src.Warehouse != null ? src.Warehouse.Name : null,
        src.ShippingProviderId,
        src.City,
        src.Country,
        src.MinDays,
        src.MaxDays,
        src.AverageDays,
        src.IsActive,
        !string.IsNullOrEmpty(src.Conditions)
        ? JsonSerializer.Deserialize<DeliveryTimeSettingsDto>(src.Conditions!, (JsonSerializerOptions?)null)
        : null,
        src.CreatedAt
        ));
        CreateMap<CreateDeliveryTimeEstimationDto, DeliveryTimeEstimation>();
        CreateMap<UpdateDeliveryTimeEstimationDto, DeliveryTimeEstimation>();

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // PickPack mappings
        // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
        CreateMap<PickPack, PickPackDto>()
        .ConstructUsing(src => new PickPackDto(
        src.Id,
        src.OrderId,
        src.Order != null ? src.Order.OrderNumber : string.Empty,
        src.WarehouseId,
        src.Warehouse != null ? src.Warehouse.Name : string.Empty,
        src.PackNumber,
        src.Status, // ✅ Enum direkt kullanılıyor (ToString() YASAK)
        src.PickedByUserId,
        src.PickedBy != null ? $"{src.PickedBy.FirstName} {src.PickedBy.LastName}" : null,
        src.PackedByUserId,
        src.PackedBy != null ? $"{src.PackedBy.FirstName} {src.PackedBy.LastName}" : null,
        src.PickedAt,
        src.PackedAt,
        src.ShippedAt,
        src.Notes,
        src.Weight,
        src.Dimensions,
        src.PackageCount,
        src.Items != null ? src.Items.Select(i => new PickPackItemDto(
        i.Id,
        i.OrderItemId,
        i.ProductId,
        i.Product != null ? i.Product.Name : string.Empty,
        i.Product != null ? i.Product.SKU : string.Empty,
        i.Quantity,
        i.IsPicked,
        i.IsPacked,
        i.PickedAt,
        i.PackedAt,
        i.Location
        )).ToList().AsReadOnly() : new List<PickPackItemDto>().AsReadOnly(),
        src.CreatedAt
        ));
        CreateMap<CreatePickPackDto, PickPack>();

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // PickPackItem mappings
        CreateMap<PickPackItem, PickPackItemDto>()
        .ConstructUsing(src => new PickPackItemDto(
        src.Id,
        src.OrderItemId,
        src.ProductId,
        src.Product != null ? src.Product.Name : string.Empty,
        src.Product != null ? src.Product.SKU : string.Empty,
        src.Quantity,
        src.IsPicked,
        src.IsPacked,
        src.PickedAt,
        src.PackedAt,
        src.Location
        ));

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // ShippingAddress mappings
        CreateMap<ShippingAddress, ShippingAddressDto>()
        .ConstructUsing(src => new ShippingAddressDto(
        src.Id,
        src.UserId,
        src.Label,
        src.FirstName,
        src.LastName,
        src.Phone,
        src.AddressLine1,
        src.AddressLine2,
        src.City,
        src.State,
        src.PostalCode,
        src.Country,
        src.IsDefault,
        src.IsActive,
        src.Instructions,
        src.CreatedAt
        ));
        CreateMap<CreateShippingAddressDto, ShippingAddress>();
        CreateMap<UpdateShippingAddressDto, ShippingAddress>();

        // SellerApplication mappings
        CreateMap<SellerApplication, SellerApplicationDto>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
        .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
        .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<CreateSellerApplicationDto, SellerApplication>();

        // Analytics mappings
        CreateMap<Report, ReportDto>()
        .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
        .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.Format.ToString()))
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
        .ForMember(dest => dest.GeneratedBy, opt => opt.MapFrom(src => 
        src.GeneratedByUser != null ? $"{src.GeneratedByUser.FirstName} {src.GeneratedByUser.LastName}" : string.Empty))
        .ForMember(dest => dest.GeneratedByUserId, opt => opt.MapFrom(src => src.GeneratedBy));

        CreateMap<ReportSchedule, ReportScheduleDto>()
        .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
        .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Frequency.ToString()))
        .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.Format.ToString()));

        CreateMap<DashboardMetric, DashboardMetricDto>()
        .ForMember(dest => dest.ValueFormatted, opt => opt.MapFrom(src => 
        src.ValueFormatted ?? src.Value.ToString("N2")));

        // B2B mappings
        CreateMap<B2BUser, B2BUserDto>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
        .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
        .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : string.Empty))
        .ForMember(dest => dest.AvailableCredit, opt => opt.MapFrom(src => 
        src.CreditLimit.HasValue && src.UsedCredit.HasValue 
        ? src.CreditLimit.Value - src.UsedCredit.Value 
        : (decimal?)null))
        .AfterMap((src, dest) => 
        {
        // ✅ BOLUM 4.3: Over-Posting Korumasi - Dictionary<string, object> YASAK
        // Typed DTO kullanılıyor
        dest.Settings = !string.IsNullOrEmpty(src.Settings) 
        ? JsonSerializer.Deserialize<B2BUserSettingsDto>(src.Settings!) 
        : null;
        });

        CreateMap<WholesalePrice, WholesalePriceDto>()
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
        .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : null));

        CreateMap<CreditTerm, CreditTermDto>()
        .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : string.Empty))
        .ForMember(dest => dest.AvailableCredit, opt => opt.MapFrom(src => 
        src.CreditLimit.HasValue && src.UsedCredit.HasValue 
        ? src.CreditLimit.Value - src.UsedCredit.Value 
        : (decimal?)null));

        CreateMap<PurchaseOrder, PurchaseOrderDto>()
        .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : string.Empty))
        .ForMember(dest => dest.B2BUserName, opt => opt.MapFrom(src => 
        src.B2BUser != null && src.B2BUser.User != null 
        ? $"{src.B2BUser.User.FirstName} {src.B2BUser.User.LastName}" 
        : null))
        .ForMember(dest => dest.CreditTermName, opt => opt.MapFrom(src => src.CreditTerm != null ? src.CreditTerm.Name : null));

        CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
        .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty));

        CreateMap<VolumeDiscount, VolumeDiscountDto>()
        .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
        .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
        .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : null));

        // PreOrder mappings
        CreateMap<PreOrder, PreOrderDto>()
        .ConstructUsing(src => new PreOrderDto(
        src.Id,
        src.UserId,
        src.ProductId,
        src.Product != null ? src.Product.Name : "Unknown",
        src.Product != null ? src.Product.ImageUrl : string.Empty,
        src.Quantity,
        src.Price,
        src.DepositAmount,
        src.DepositPaid,
        src.Status, // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
        src.ExpectedAvailabilityDate,
        src.ActualAvailabilityDate,
        src.ExpiresAt,
        src.Notes,
        src.CreatedAt
        ));

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        CreateMap<PreOrderCampaign, PreOrderCampaignDto>()
        .ConstructUsing(src => new PreOrderCampaignDto(
        src.Id,
        src.Name,
        src.Description,
        src.ProductId,
        src.Product != null ? src.Product.Name : "Unknown",
        src.Product != null ? src.Product.ImageUrl : string.Empty,
        src.StartDate,
        src.EndDate,
        src.ExpectedDeliveryDate,
        src.MaxQuantity,
        src.CurrentQuantity,
        src.MaxQuantity > 0 ? src.MaxQuantity - src.CurrentQuantity : int.MaxValue,
        src.DepositPercentage,
        src.SpecialPrice,
        src.IsActive,
        src.MaxQuantity > 0 && src.CurrentQuantity >= src.MaxQuantity
        ));

        // AbandonedCartEmail mappings
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        CreateMap<AbandonedCartEmail, AbandonedCartEmailDto>()
        .ConstructUsing(src => new AbandonedCartEmailDto(
        src.Id,
        src.CartId,
        src.UserId,
        src.EmailType,
        src.SentAt,
        src.WasOpened,
        src.WasClicked,
        src.ResultedInPurchase,
        src.CouponId,
        src.Coupon != null ? src.Coupon.Code : null
        ));

        // Cart -> AbandonedCartDto mapping (complex mapping with computed properties)
        CreateMap<Merge.Domain.Modules.Ordering.Cart, AbandonedCartDto>()
        .ForMember(dest => dest.CartId, opt => opt.MapFrom(src => src.Id))
        .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
        .ForMember(dest => dest.ItemCount, opt => opt.Ignore()) // Set manually
        .ForMember(dest => dest.TotalValue, opt => opt.Ignore()) // Set manually
        .ForMember(dest => dest.LastModified, opt => opt.Ignore()) // Set manually
        .ForMember(dest => dest.HoursSinceAbandonment, opt => opt.Ignore()) // Set manually
        .ForMember(dest => dest.Items, opt => opt.Ignore()) // Set manually
        .ForMember(dest => dest.HasReceivedEmail, opt => opt.Ignore()) // Set manually
        .ForMember(dest => dest.EmailsSentCount, opt => opt.Ignore()) // Set manually
        .ForMember(dest => dest.LastEmailSent, opt => opt.Ignore()); // Set manually


    }
}
