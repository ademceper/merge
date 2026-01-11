using AutoMapper;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Cart;
using Merge.Application.DTOs.Catalog;
using Merge.Application.DTOs.Logistics;
using Merge.Application.DTOs.Marketing;
using Merge.Application.DTOs.Notification;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Payment;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Review;
using Merge.Application.DTOs.Seller;
using Merge.Application.DTOs.Support;
using Merge.Application.DTOs.User;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.B2B;
using Merge.Application.DTOs.Content;
using Merge.Application.DTOs.Governance;
using Merge.Application.DTOs.Security;
using Merge.Application.DTOs.International;
using Merge.Application.DTOs.Identity;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.DTOs.Organization;
using Merge.Application.DTOs.Search;
using Merge.Application.DTOs.Subscription;
using System.Text.Json;
using OrganizationEntity = Merge.Domain.Entities.Organization;
using ReviewEntity = Merge.Domain.Entities.Review;
using ProductEntity = Merge.Domain.Entities.Product;

namespace Merge.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        CreateMap<Merge.Domain.Entities.Product, ProductDto>()
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
                src.ImageUrls.AsReadOnly(),
                src.Rating,
                src.ReviewCount,
                src.IsActive,
                src.CategoryId,
                src.Category != null ? src.Category.Name : string.Empty,
                src.SellerId,
                src.StoreId
            ));

        // ProductDto → Product mapping (Create/Update için)
        CreateMap<ProductDto, Merge.Domain.Entities.Product>()
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

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
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

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        CreateMap<Merge.Domain.Entities.User, UserDto>()
            .ConstructUsing(src => new UserDto(
                src.Id,
                src.FirstName,
                src.LastName,
                src.Email ?? string.Empty,
                src.PhoneNumber ?? string.Empty,
                string.Empty)); // Role handler'da set edilecek

        CreateMap<Address, AddressDto>();
        CreateMap<AddressDto, Address>()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore());

        // User Preference mappings
        CreateMap<UserPreference, UserPreferenceDto>();

        // User Activity Log mappings
        CreateMap<UserActivityLog, UserActivityLogDto>()
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => 
                src.User != null ? src.User.Email : "Anonymous"));

        // ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı) - Positional constructor kullanımı
        CreateMap<CartItem, CartItemDto>()
            .ConstructUsing(src => new CartItemDto(
                src.Id,
                src.ProductId,
                src.Product != null ? src.Product.Name : string.Empty,
                src.Product != null ? src.Product.ImageUrl : string.Empty,
                src.Quantity,
                src.Price,
                src.Quantity * src.Price
            ));

        CreateMap<Merge.Domain.Entities.Cart, CartDto>()
            .ConstructUsing(src => new CartDto(
                src.Id,
                src.UserId,
                src.CartItems.Select(ci => new CartItemDto(
                    ci.Id,
                    ci.ProductId,
                    ci.Product != null ? ci.Product.Name : string.Empty,
                    ci.Product != null ? ci.Product.ImageUrl : string.Empty,
                    ci.Quantity,
                    ci.Price,
                    ci.Quantity * ci.Price
                )).ToList().AsReadOnly(),
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

        CreateMap<Merge.Domain.Entities.Order, OrderDto>()
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
        CreateMap<Merge.Domain.Entities.Notification, NotificationDto>()
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

        CreateMap<Merge.Domain.Entities.Payment, PaymentDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : string.Empty));
        CreateMap<CreatePaymentDto, Merge.Domain.Entities.Payment>();

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        CreateMap<Shipping, ShippingDto>()
            .ConstructUsing(src => new ShippingDto(
                src.Id,
                src.OrderId,
                src.Order != null ? src.Order.OrderNumber : string.Empty,
                src.ShippingProvider,
                src.TrackingNumber,
                src.Status.ToString(),
                src.ShippedDate,
                src.EstimatedDeliveryDate,
                src.DeliveredDate,
                src.ShippingCost,
                src.ShippingLabelUrl,
                src.CreatedAt
            ));
        CreateMap<CreateShippingDto, Shipping>();

        CreateMap<CreateAddressDto, Address>();
        CreateMap<UpdateAddressDto, Address>();

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
        CreateMap<Merge.Domain.Entities.Product, ComparisonProductDto>()
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
        CreateMap<Merge.Domain.Entities.Product, ProductSuggestionDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.DiscountPrice ?? src.Price));

        // Product → ProductRecommendationDto mapping
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        CreateMap<Merge.Domain.Entities.Product, ProductRecommendationDto>()
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
        CreateMap<FAQ, FaqDto>().ReverseMap();
        CreateMap<CreateFaqDto, FAQ>();
        CreateMap<UpdateFaqDto, FAQ>();

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
        CreateMap<Inventory, InventoryDto>()
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
        CreateMap<CreateInventoryDto, Inventory>();
        CreateMap<UpdateInventoryDto, Inventory>();

        // LowStockAlert mappings
        CreateMap<Inventory, LowStockAlertDto>()
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

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // PickPack mappings
        CreateMap<PickPack, PickPackDto>()
            .ConstructUsing(src => new PickPackDto(
                src.Id,
                src.OrderId,
                src.Order != null ? src.Order.OrderNumber : string.Empty,
                src.WarehouseId,
                src.Warehouse != null ? src.Warehouse.Name : string.Empty,
                src.PackNumber,
                src.Status.ToString(),
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
        CreateMap<Merge.Domain.Entities.Cart, AbandonedCartDto>()
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

        // Content Domain Mappings
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // Blog mappings
        CreateMap<BlogCategory, BlogCategoryDto>()
            .ConstructUsing(src => new BlogCategoryDto(
                src.Id,
                src.Name,
                src.Slug,
                src.Description,
                src.ParentCategoryId,
                src.ParentCategory != null ? src.ParentCategory.Name : null,
                null, // SubCategories - Set manually (recursive)
                0, // PostCount - Set manually (batch loading)
                src.ImageUrl,
                src.DisplayOrder,
                src.IsActive,
                src.CreatedAt
            ));

        CreateMap<BlogPost, BlogPostDto>()
            .ConstructUsing(src => new BlogPostDto(
                src.Id,
                src.CategoryId,
                src.Category != null ? src.Category.Name : string.Empty,
                src.AuthorId,
                src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : string.Empty,
                src.Title,
                src.Slug,
                src.Excerpt,
                src.Content,
                src.FeaturedImageUrl,
                src.Status.ToString(),
                src.PublishedAt,
                src.ViewCount,
                src.LikeCount,
                src.CommentCount,
                !string.IsNullOrEmpty(src.Tags) ? src.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList().AsReadOnly() : new List<string>().AsReadOnly(),
                src.IsFeatured,
                src.AllowComments,
                src.MetaTitle,
                src.MetaDescription,
                src.MetaKeywords,
                src.OgImageUrl,
                src.ReadingTimeMinutes,
                src.CreatedAt
            ));

        CreateMap<BlogComment, BlogCommentDto>()
            .ConstructUsing(src => new BlogCommentDto(
                src.Id,
                src.BlogPostId,
                src.UserId,
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : src.AuthorName,
                src.ParentCommentId,
                src.AuthorName,
                src.Content,
                src.IsApproved,
                src.LikeCount,
                0, // ReplyCount - Set manually (computed from Replies)
                null, // Replies - Set manually (recursive)
                src.CreatedAt
            ));

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // CMS mappings
        CreateMap<CMSPage, CMSPageDto>()
            .ConstructUsing(src => new CMSPageDto(
                src.Id,
                src.Title,
                src.Slug,
                src.Content,
                src.Excerpt,
                src.PageType,
                src.Status.ToString(),
                src.AuthorId,
                src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : null,
                src.PublishedAt,
                src.Template,
                src.MetaTitle,
                src.MetaDescription,
                src.MetaKeywords,
                src.IsHomePage,
                src.DisplayOrder,
                src.ShowInMenu,
                src.MenuTitle,
                src.ParentPageId,
                src.ParentPage != null ? src.ParentPage.Title : null,
                src.ViewCount,
                src.CreatedAt
            ));

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // LandingPage mappings
        CreateMap<LandingPage, LandingPageDto>()
            .ConstructUsing(src => new LandingPageDto(
                src.Id,
                src.Name,
                src.Slug,
                src.Title,
                src.Content,
                src.Template,
                src.Status.ToString(),
                src.AuthorId,
                src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : null,
                src.PublishedAt,
                src.StartDate,
                src.EndDate,
                src.IsActive,
                src.MetaTitle,
                src.MetaDescription,
                src.OgImageUrl,
                src.ViewCount,
                src.ConversionCount,
                src.ConversionRate,
                src.EnableABTesting,
                src.VariantOfId,
                src.TrafficSplit,
                src.CreatedAt
            ));

        CreateMap<LandingPage, LandingPageVariantDto>()
            .ConstructUsing(src => new LandingPageVariantDto(
                src.Id,
                src.Name,
                src.ViewCount,
                src.ConversionCount,
                src.ConversionRate,
                src.TrafficSplit
            ));

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // PageBuilder mappings
        CreateMap<PageBuilder, PageBuilderDto>()
            .ConstructUsing(src => new PageBuilderDto(
                src.Id,
                src.Name,
                src.Slug,
                src.Title,
                src.Content,
                src.Template,
                src.Status.ToString(),
                src.AuthorId,
                src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : null,
                src.PublishedAt,
                src.IsActive,
                src.MetaTitle,
                src.MetaDescription,
                src.OgImageUrl,
                src.ViewCount,
                src.PageType,
                src.RelatedEntityId,
                src.CreatedAt
            ));

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // SEO mappings
        // ✅ BOLUM 4.3: Over-Posting Koruması - Dictionary<string, object> YASAK
        // StructuredData artık string olarak map ediliyor (StructuredDataJson)
        CreateMap<SEOSettings, SEOSettingsDto>()
            .ConstructUsing(src => new SEOSettingsDto(
                src.Id,
                src.PageType,
                src.EntityId,
                src.MetaTitle,
                src.MetaDescription,
                src.MetaKeywords,
                src.CanonicalUrl,
                src.OgTitle,
                src.OgDescription,
                src.OgImageUrl,
                src.TwitterCard,
                src.StructuredData, // JSON string
                src.IsIndexed,
                src.FollowLinks,
                src.Priority,
                src.ChangeFrequency,
                src.CreatedAt
            ));

        CreateMap<SitemapEntry, SitemapEntryDto>()
            .ConstructUsing(src => new SitemapEntryDto(
                src.Id,
                src.Url,
                src.PageType,
                src.EntityId,
                src.LastModified,
                src.ChangeFrequency,
                src.Priority,
                src.IsActive
            ));

        // Governance mappings
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // Governance mappings
        CreateMap<Policy, PolicyDto>()
            .ConstructUsing(src => new PolicyDto(
                src.Id,
                src.PolicyType,
                src.Title,
                src.Content,
                src.Version,
                src.IsActive,
                src.RequiresAcceptance,
                src.EffectiveDate,
                src.ExpiryDate,
                src.CreatedByUserId,
                src.CreatedBy != null ? $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}" : null,
                src.ChangeLog,
                src.Language,
                0, // AcceptanceCount - Set manually (computed)
                src.CreatedAt,
                src.UpdatedAt));

        CreateMap<PolicyAcceptance, PolicyAcceptanceDto>()
            .ConstructUsing(src => new PolicyAcceptanceDto(
                src.Id,
                src.PolicyId,
                src.Policy != null ? src.Policy.Title : string.Empty,
                src.UserId,
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty,
                src.AcceptedVersion,
                src.IpAddress,
                src.AcceptedAt,
                src.IsActive));

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // AuditLog mapping
        CreateMap<AuditLog, AuditLogDto>()
            .ConstructUsing(src => new AuditLogDto(
                src.Id,
                src.UserId,
                src.UserEmail,
                src.Action,
                src.EntityType,
                src.EntityId,
                src.TableName,
                src.PrimaryKey,
                src.OldValues,
                src.NewValues,
                src.Changes,
                src.IpAddress,
                src.UserAgent,
                src.Severity.ToString(),
                src.Module,
                src.IsSuccessful,
                src.ErrorMessage,
                src.CreatedAt));

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

        // Identity mappings
        // User -> UserDto mapping zaten var (line 47)
        // TwoFactorAuth -> TwoFactorStatusDto için özel mapping (MaskPhoneNumber, MaskEmail, BackupCodesRemaining)
        CreateMap<TwoFactorAuth, TwoFactorStatusDto>()
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore()) // Set manually (MaskPhoneNumber)
            .ForMember(dest => dest.Email, opt => opt.Ignore()) // Set manually (MaskEmail)
            .ForMember(dest => dest.BackupCodesRemaining, opt => opt.Ignore()); // Set manually (Array.Length)

        // LiveCommerce mappings
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        CreateMap<LiveStream, LiveStreamDto>()
            .ConstructUsing(src => new LiveStreamDto(
                src.Id,
                src.SellerId,
                src.Seller != null ? src.Seller.StoreName : string.Empty,
                src.Title,
                src.Description,
                src.Status.ToString(),
                src.ScheduledStartTime,
                src.ActualStartTime,
                src.EndTime,
                src.StreamUrl,
                src.StreamKey,
                src.ThumbnailUrl,
                src.ViewerCount,
                src.PeakViewerCount,
                src.TotalViewerCount,
                src.OrderCount,
                src.Revenue,
                src.IsActive,
                src.Category,
                src.Tags,
                src.Products != null ? src.Products.Select(p => new LiveStreamProductDto(
                    p.Id,
                    p.ProductId,
                    p.Product != null ? p.Product.Name : string.Empty,
                    p.Product != null ? p.Product.ImageUrl : null,
                    p.Product != null ? p.Product.Price : null,
                    p.SpecialPrice,
                    p.DisplayOrder,
                    p.IsHighlighted,
                    p.ShowcasedAt,
                    p.ViewCount,
                    p.ClickCount,
                    p.OrderCount,
                    p.ShowcaseNotes
                )).ToList().AsReadOnly() : new List<LiveStreamProductDto>().AsReadOnly(),
                src.CreatedAt,
                src.UpdatedAt));

        CreateMap<LiveStreamProduct, LiveStreamProductDto>()
            .ConstructUsing(src => new LiveStreamProductDto(
                src.Id,
                src.ProductId,
                src.Product != null ? src.Product.Name : string.Empty,
                src.Product != null ? src.Product.ImageUrl : null,
                src.Product != null ? src.Product.Price : null,
                src.SpecialPrice,
                src.DisplayOrder,
                src.IsHighlighted,
                src.ShowcasedAt,
                src.ViewCount,
                src.ClickCount,
                src.OrderCount,
                src.ShowcaseNotes));

        CreateMap<LiveStreamOrder, LiveStreamOrderDto>()
            .ConstructUsing(src => new LiveStreamOrderDto(
                src.Id,
                src.LiveStreamId,
                src.OrderId,
                src.ProductId,
                src.OrderAmount,
                src.CreatedAt));

        // Marketing - Email Campaign mappings
        CreateMap<EmailCampaign, EmailCampaignDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
        CreateMap<CreateEmailCampaignDto, EmailCampaign>();
        CreateMap<UpdateEmailCampaignDto, EmailCampaign>();

        CreateMap<EmailCampaign, EmailCampaignAnalyticsDto>()
            .ForMember(dest => dest.CampaignId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CampaignName, opt => opt.MapFrom(src => src.Name));

        CreateMap<EmailTemplate, EmailTemplateDto>()
            .ConvertUsing(src => new EmailTemplateDto(
                src.Id,
                src.Name,
                src.Description,
                src.Subject,
                src.HtmlContent,
                src.Type.ToString(),
                src.IsActive,
                src.Thumbnail,
                !string.IsNullOrEmpty(src.Variables)
                    ? JsonSerializer.Deserialize<List<string>>(src.Variables, (JsonSerializerOptions?)null) ?? new List<string>()
                    : new List<string>()));
        CreateMap<CreateEmailTemplateDto, EmailTemplate>();

        CreateMap<EmailSubscriber, EmailSubscriberDto>()
            .ConvertUsing(src => new EmailSubscriberDto(
                src.Id,
                src.Email,
                src.FirstName,
                src.LastName,
                src.IsSubscribed,
                src.SubscribedAt,
                src.UnsubscribedAt,
                src.Source,
                src.EmailsSent,
                src.EmailsOpened,
                src.EmailsClicked,
                !string.IsNullOrEmpty(src.Tags)
                    ? JsonSerializer.Deserialize<List<string>>(src.Tags, (JsonSerializerOptions?)null) ?? new List<string>()
                    : new List<string>()));
        CreateMap<CreateEmailSubscriberDto, EmailSubscriber>();

        CreateMap<EmailAutomation, EmailAutomationDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.TemplateName, opt => opt.MapFrom(src => src.Template != null ? src.Template.Name : "Unknown"));
        CreateMap<CreateEmailAutomationDto, EmailAutomation>();

        // Marketing - Loyalty mappings
        CreateMap<LoyaltyAccount, LoyaltyAccountDto>()
            .ForMember(dest => dest.TierName, opt => opt.MapFrom(src => src.Tier != null ? src.Tier.Name : "No Tier"))
            .ForMember(dest => dest.TierLevel, opt => opt.MapFrom(src => src.Tier != null ? src.Tier.Level : 0));

        CreateMap<LoyaltyTransaction, LoyaltyTransactionDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        CreateMap<LoyaltyTier, LoyaltyTierDto>();

        // Organization mappings
        // ✅ BOLUM 7.1.5: Records - ConvertUsing ile record mapping (immutable DTOs + error handling)
        // ✅ FIX: Expression tree limitation - ConvertUsing kullanıyoruz (statement body destekleniyor)
        CreateMap<OrganizationEntity, OrganizationDto>()
            .ConvertUsing((src, context) =>
            {
                OrganizationSettingsDto? settings = null;
                if (!string.IsNullOrEmpty(src.Settings))
                {
                    try
                    {
                        settings = JsonSerializer.Deserialize<OrganizationSettingsDto>(src.Settings);
                    }
                    catch
                    {
                        // ✅ ERROR HANDLING: JSON deserialize hatası - null bırak
                    }
                }

                return new OrganizationDto(
                    src.Id,
                    src.Name,
                    src.LegalName,
                    src.TaxNumber,
                    src.RegistrationNumber,
                    src.Email,
                    src.Phone,
                    src.Website,
                    src.Address,
                    src.City,
                    src.State,
                    src.PostalCode,
                    src.Country,
                    src.Status.ToString(),
                    src.IsVerified,
                    src.VerifiedAt,
                    settings, // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
                    0, // UserCount - Service layer'da set edilecek
                    0, // TeamCount - Service layer'da set edilecek
                    src.CreatedAt);
            });

        // ✅ FIX: Expression tree limitation - ConvertUsing kullanıyoruz (statement body destekleniyor)
        CreateMap<Team, TeamDto>()
            .ConvertUsing((src, context) =>
            {
                TeamSettingsDto? settings = null;
                if (!string.IsNullOrEmpty(src.Settings))
                {
                    try
                    {
                        settings = JsonSerializer.Deserialize<TeamSettingsDto>(src.Settings);
                    }
                    catch
                    {
                        // ✅ ERROR HANDLING: JSON deserialize hatası - null bırak
                    }
                }

                return new TeamDto(
                    src.Id,
                    src.OrganizationId,
                    src.Organization != null ? src.Organization.Name : string.Empty,
                    src.Name,
                    src.Description,
                    src.TeamLeadId,
                    src.TeamLead != null ? $"{src.TeamLead.FirstName} {src.TeamLead.LastName}" : null,
                    src.IsActive,
                    settings, // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
                    0, // MemberCount - Service layer'da set edilecek
                    src.CreatedAt);
            });

        CreateMap<TeamMember, TeamMemberDto>()
            .ConstructUsing(src => new TeamMemberDto(
                src.Id,
                src.TeamId,
                src.Team != null ? src.Team.Name : string.Empty,
                src.UserId,
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty,
                src.User != null ? src.User.Email ?? string.Empty : string.Empty,
                src.Role,
                src.JoinedAt,
                src.IsActive));

        // Marketing - Referral mappings
        CreateMap<ReferralCode, ReferralCodeDto>();
        CreateMap<CreateReferralDto, ReferralCode>();

        CreateMap<Referral, ReferralDto>()
            .ForMember(dest => dest.ReferredUserEmail, opt => opt.MapFrom(src => src.ReferredUser != null ? src.ReferredUser.Email : string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        // Marketing - ReviewMedia mappings
        CreateMap<ReviewMedia, ReviewMediaDto>()
            .ForMember(dest => dest.MediaType, opt => opt.MapFrom(src => src.MediaType.ToString()));

        // Marketing - SharedWishlist mappings
        CreateMap<SharedWishlist, SharedWishlistDto>();
        CreateMap<CreateSharedWishlistDto, SharedWishlist>();

        CreateMap<SharedWishlistItem, SharedWishlistItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product != null ? (src.Product.DiscountPrice ?? src.Product.Price) : 0))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : string.Empty));

        // ML - Fraud Detection mappings
        CreateMap<FraudDetectionRule, FraudDetectionRuleDto>()
            .AfterMap((src, dest) =>
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
                dest.Conditions = !string.IsNullOrEmpty(src.Conditions)
                    ? JsonSerializer.Deserialize<FraudRuleConditionsDto>(src.Conditions)
                    : null;
            });
        CreateMap<CreateFraudDetectionRuleDto, FraudDetectionRule>();

        CreateMap<FraudAlert, FraudAlertDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null))
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : null))
            .ForMember(dest => dest.ReviewedByName, opt => opt.MapFrom(src => src.ReviewedBy != null ? $"{src.ReviewedBy.FirstName} {src.ReviewedBy.LastName}" : null))
            .AfterMap((src, dest) =>
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.MatchedRules = !string.IsNullOrEmpty(src.MatchedRules)
                    ? JsonSerializer.Deserialize<List<Guid>>(src.MatchedRules)
                    : null;
            });

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // ✅ BOLUM 1.2: Enum kullanımı (string NotificationType/Channel YASAK)
        // ✅ FIX: Expression tree limitation - ConvertUsing kullanıyoruz
        CreateMap<NotificationPreference, NotificationPreferenceDto>()
            .ConvertUsing((src, context) =>
            {
                NotificationPreferenceSettingsDto? customSettings = null;
                if (!string.IsNullOrEmpty(src.CustomSettings))
                {
                    try
                    {
                        customSettings = JsonSerializer.Deserialize<NotificationPreferenceSettingsDto>(src.CustomSettings);
                    }
                    catch
                    {
                        // JSON deserialize hatası - null bırak
                    }
                }
                return new NotificationPreferenceDto(
                    src.Id,
                    src.UserId,
                    src.NotificationType,
                    src.Channel,
                    src.IsEnabled,
                    customSettings,
                    src.CreatedAt,
                    src.UpdatedAt);
            });
        
        // CreateNotificationPreferenceDto artık command'da kullanılıyor, mapping gerekmiyor

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // ✅ BOLUM 1.2: Enum kullanımı (string Type YASAK)
        // ✅ FIX: Expression tree limitation - ConvertUsing kullanıyoruz (statement body destekleniyor)
        CreateMap<NotificationTemplate, NotificationTemplateDto>()
            .ConvertUsing((src, context) =>
            {
                NotificationVariablesDto? variables = null;
                if (!string.IsNullOrEmpty(src.Variables))
                {
                    try
                    {
                        variables = JsonSerializer.Deserialize<NotificationVariablesDto>(src.Variables);
                    }
                    catch
                    {
                        // JSON deserialize hatası - null bırak
                    }
                }

                NotificationTemplateSettingsDto? defaultData = null;
                if (!string.IsNullOrEmpty(src.DefaultData))
                {
                    try
                    {
                        defaultData = JsonSerializer.Deserialize<NotificationTemplateSettingsDto>(src.DefaultData);
                    }
                    catch
                    {
                        // JSON deserialize hatası - null bırak
                    }
                }

                return new NotificationTemplateDto(
                    src.Id,
                    src.Name,
                    src.Description,
                    src.Type,
                    src.TitleTemplate,
                    src.MessageTemplate,
                    src.LinkTemplate,
                    src.IsActive,
                    variables,
                    defaultData,
                    src.CreatedAt,
                    src.UpdatedAt);
            });
        
        // CreateNotificationTemplateDto ve UpdateNotificationTemplateDto artık command'da kullanılıyor, mapping gerekmiyor

        // Order Split mappings
        CreateMap<OrderSplitItem, OrderSplitItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => 
                src.OriginalOrderItem != null && src.OriginalOrderItem.Product != null 
                    ? src.OriginalOrderItem.Product.Name 
                    : string.Empty));

        CreateMap<OrderSplit, OrderSplitDto>()
            .ForMember(dest => dest.OriginalOrderNumber, opt => opt.MapFrom(src => 
                src.OriginalOrder != null ? src.OriginalOrder.OrderNumber : string.Empty))
            .ForMember(dest => dest.SplitOrderNumber, opt => opt.MapFrom(src => 
                src.SplitOrder != null ? src.SplitOrder.OrderNumber : string.Empty))
            .ForMember(dest => dest.SplitItems, opt => opt.MapFrom(src => 
                src.OrderSplitItems != null 
                    ? src.OrderSplitItems.ToList() 
                    : new List<OrderSplitItem>()));

        // Security mappings
        // ✅ BOLUM 1.2: Enum kullanımı (string EventType/Severity/Status YASAK) - Enum'ları string'e çevir
        CreateMap<OrderVerification, OrderVerificationDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => 
                src.Order != null ? src.Order.OrderNumber : string.Empty))
            .ForMember(dest => dest.VerifiedByName, opt => opt.MapFrom(src => 
                src.VerifiedBy != null ? $"{src.VerifiedBy.FirstName} {src.VerifiedBy.LastName}" : null))
            .ForMember(dest => dest.VerificationType, opt => opt.MapFrom(src => src.VerificationType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<PaymentFraudPrevention, PaymentFraudPreventionDto>()
            .ForMember(dest => dest.PaymentTransactionId, opt => opt.MapFrom(src =>
                src.Payment != null ? src.Payment.TransactionId : string.Empty))
            .ForMember(dest => dest.CheckType, opt => opt.MapFrom(src => src.CheckType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .AfterMap((src, dest) =>
            {
                // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
                dest.CheckResult = !string.IsNullOrEmpty(src.CheckResult)
                    ? JsonSerializer.Deserialize<FraudDetectionMetadataDto>(src.CheckResult)
                    : null;
            });

        CreateMap<AccountSecurityEvent, AccountSecurityEventDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
            .ForMember(dest => dest.ActionTakenByName, opt => opt.MapFrom(src =>
                src.ActionTakenBy != null ? $"{src.ActionTakenBy.FirstName} {src.ActionTakenBy.LastName}" : null))
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType.ToString()))
            .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity.ToString()))
            .AfterMap((src, dest) =>
            {
                // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
                dest.Details = !string.IsNullOrEmpty(src.Details)
                    ? JsonSerializer.Deserialize<SecurityEventMetadataDto>(src.Details)
                    : null;
            });

        CreateMap<SecurityAlert, SecurityAlertDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null))
            .ForMember(dest => dest.AcknowledgedByName, opt => opt.MapFrom(src =>
                src.AcknowledgedBy != null ? $"{src.AcknowledgedBy.FirstName} {src.AcknowledgedBy.LastName}" : null))
            .ForMember(dest => dest.ResolvedByName, opt => opt.MapFrom(src =>
                src.ResolvedBy != null ? $"{src.ResolvedBy.FirstName} {src.ResolvedBy.LastName}" : null))
            .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .AfterMap((src, dest) =>
            {
                // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
                dest.Metadata = !string.IsNullOrEmpty(src.Metadata)
                    ? JsonSerializer.Deserialize<SecurityEventMetadataDto>(src.Metadata)
                    : null;
            });

        // Seller domain mappings
        CreateMap<SellerTransaction, SellerTransactionDto>();

        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        CreateMap<SellerInvoice, SellerInvoiceDto>()
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
                src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : string.Empty))
            // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - Status enum'dan enum'a direkt map edilir
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .AfterMap((src, dest) =>
            {
                dest.Items = !string.IsNullOrEmpty(src.InvoiceData)
                    ? JsonSerializer.Deserialize<List<InvoiceItemDto>>(src.InvoiceData) ?? new List<InvoiceItemDto>()
                    : new List<InvoiceItemDto>();
            });

        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        CreateMap<SellerCommission, SellerCommissionDto>()
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
                src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : "Unknown"))
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => 
                src.Order != null ? src.Order.OrderNumber : "Unknown"))
            // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - Status enum'dan enum'a direkt map edilir
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
        CreateMap<CommissionPayout, CommissionPayoutDto>()
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
                src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : "Unknown"))
            // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - Status enum'dan enum'a direkt map edilir
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.Commissions, opt => opt.MapFrom(src => 
                src.Items != null && src.Items.Any() 
                    ? src.Items.Where(i => i.Commission != null).Select(i => i.Commission!)
                    : new List<SellerCommission>()));

        CreateMap<CommissionTier, CommissionTierDto>();

        CreateMap<SellerCommissionSettings, SellerCommissionSettingsDto>();

        CreateMap<Store, StoreDto>()
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src =>
                src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : string.Empty))
            .ForMember(dest => dest.ProductCount, opt => opt.Ignore()) // Will be set in StoreService after batch loading
            .AfterMap((src, dest) =>
            {
                // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
                dest.Settings = !string.IsNullOrEmpty(src.Settings)
                    ? JsonSerializer.Deserialize<StoreSettingsDto>(src.Settings)
                    : null;
            });

        // Subscription Domain Mappings
        CreateMap<SubscriptionPlan, SubscriptionPlanDto>()
            .ForMember(dest => dest.Features, opt => opt.Ignore()) // Will be set in AfterMap
            .ForMember(dest => dest.SubscriberCount, opt => opt.Ignore()) // Will be set in SubscriptionService after batch loading
            .AfterMap((src, dest) =>
            {
                // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
                dest.Features = !string.IsNullOrEmpty(src.Features)
                    ? JsonSerializer.Deserialize<SubscriptionPlanFeaturesDto>(src.Features)
                    : null;
            });

        CreateMap<UserSubscription, UserSubscriptionDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
            .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src =>
                src.SubscriptionPlan != null ? src.SubscriptionPlan.Name : string.Empty))
            .ForMember(dest => dest.IsTrial, opt => opt.MapFrom(src => src.Status == SubscriptionStatus.Trial))
            .ForMember(dest => dest.DaysRemaining, opt => opt.MapFrom(src =>
                src.EndDate > DateTime.UtcNow ? (int)(src.EndDate - DateTime.UtcNow).TotalDays : 0))
            .ForMember(dest => dest.RecentPayments, opt => opt.Ignore()); // Will be set in SubscriptionService after batch loading

        CreateMap<SubscriptionPayment, SubscriptionPaymentDto>();

        CreateMap<SubscriptionUsage, SubscriptionUsageDto>()
            .ForMember(dest => dest.Remaining, opt => opt.MapFrom(src =>
                src.Limit.HasValue ? src.Limit.Value - src.UsageCount : (int?)null));

        // Support Domain Mappings
        CreateMap<SupportTicket, SupportTicketDto>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : "Unknown"))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : null))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src =>
                src.AssignedTo != null ? $"{src.AssignedTo.FirstName} {src.AssignedTo.LastName}" : null))
            .ForMember(dest => dest.Messages, opt => opt.Ignore()) // Will be set in SupportTicketService after batch loading
            .ForMember(dest => dest.Attachments, opt => opt.Ignore()); // Will be set in SupportTicketService after batch loading

        CreateMap<TicketMessage, TicketMessageDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : "Unknown"))
            .ForMember(dest => dest.Attachments, opt => opt.Ignore()); // Will be set in SupportTicketService after batch loading

        CreateMap<TicketAttachment, TicketAttachmentDto>();

        CreateMap<CustomerCommunication, CustomerCommunicationDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
            .ForMember(dest => dest.SentByName, opt => opt.MapFrom(src =>
                src.SentBy != null ? $"{src.SentBy.FirstName} {src.SentBy.LastName}" : null))
            .ForMember(dest => dest.Metadata, opt => opt.Ignore()) // Will be set in AfterMap
            .AfterMap((src, dest) =>
            {
                // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
                dest.Metadata = !string.IsNullOrEmpty(src.Metadata)
                    ? JsonSerializer.Deserialize<CustomerCommunicationSettingsDto>(src.Metadata)
                    : null;
            });

        CreateMap<KnowledgeBaseArticle, KnowledgeBaseArticleDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src =>
                src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : null))
            .ForMember(dest => dest.Tags, opt => opt.Ignore()) // Will be set in AfterMap
            .AfterMap((src, dest) =>
            {
                dest.Tags = !string.IsNullOrEmpty(src.Tags)
                    ? src.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    : new List<string>();
            });

        CreateMap<KnowledgeBaseCategory, KnowledgeBaseCategoryDto>()
            .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
            .ForMember(dest => dest.ArticleCount, opt => opt.Ignore()) // Will be set in KnowledgeBaseService after batch loading
            .ForMember(dest => dest.SubCategories, opt => opt.Ignore()); // Will be set in KnowledgeBaseService recursively

        CreateMap<LiveChatSession, LiveChatSessionDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : src.GuestName))
            .ForMember(dest => dest.AgentName, opt => opt.MapFrom(src =>
                src.Agent != null ? $"{src.Agent.FirstName} {src.Agent.LastName}" : null))
            .ForMember(dest => dest.Tags, opt => opt.Ignore()) // Will be set in AfterMap
            .ForMember(dest => dest.RecentMessages, opt => opt.Ignore()) // Will be set in LiveChatService after batch loading
            .AfterMap((src, dest) =>
            {
                dest.Tags = !string.IsNullOrEmpty(src.Tags)
                    ? src.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                    : new List<string>();
            });

        CreateMap<LiveChatMessage, LiveChatMessageDto>()
            .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src =>
                src.Sender != null ? $"{src.Sender.FirstName} {src.Sender.LastName}" : null));
    }
}

