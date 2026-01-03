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

namespace Merge.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

        CreateMap<ProductDto, Product>()
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
            .ForMember(dest => dest.CartItems, opt => opt.Ignore())
            .ForMember(dest => dest.Reviews, opt => opt.Ignore())
            .ForMember(dest => dest.Seller, opt => opt.Ignore());

        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null));

        CreateMap<CategoryDto, Category>()
            .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
            .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
            .ForMember(dest => dest.Products, opt => opt.Ignore());

        CreateMap<User, UserDto>();

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

        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product.ImageUrl))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Quantity * src.Price));

        CreateMap<Cart, CartDto>()
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount));

        CreateMap<SavedCartItem, SavedCartItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : string.Empty))
            .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src => src.Product != null ? (src.Product.DiscountPrice ?? src.Product.Price) : 0))
            .ForMember(dest => dest.IsPriceChanged, opt => opt.MapFrom(src => 
                src.Product != null && src.Price != (src.Product.DiscountPrice ?? src.Product.Price)));

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : string.Empty))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.UnitPrice))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice));

        CreateMap<Order, OrderDto>()
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

        CreateMap<Notification, NotificationDto>();
        CreateMap<CreateNotificationDto, Notification>();

        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));
        CreateMap<CreateReviewDto, Review>();
        CreateMap<UpdateReviewDto, Review>();

        // TrustBadge mappings
        CreateMap<TrustBadge, TrustBadgeDto>()
            .AfterMap((src, dest) =>
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.Criteria = !string.IsNullOrEmpty(src.Criteria)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Criteria)
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
        CreateMap<Review, ReviewHelpfulnessStatsDto>()
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

        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : string.Empty));
        CreateMap<CreatePaymentDto, Payment>();

        CreateMap<Shipping, ShippingDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : string.Empty));
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
        CreateMap<ProductBundle, ProductBundleDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => 
                src.BundleItems != null 
                    ? src.BundleItems.OrderBy(bi => bi.SortOrder).ToList() 
                    : new List<BundleItem>()));
        CreateMap<CreateProductBundleDto, ProductBundle>();
        CreateMap<UpdateProductBundleDto, ProductBundle>();

        // BundleItem mappings
        CreateMap<BundleItem, BundleItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : string.Empty))
            .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => 
                src.Product != null ? (src.Product.DiscountPrice ?? src.Product.Price) : 0));

        // ProductQuestion mappings
        CreateMap<ProductQuestion, ProductQuestionDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
            .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => 
                src.Answers != null ? src.Answers.Where(a => a.IsApproved).ToList() : new List<ProductAnswer>()))
            .ForMember(dest => dest.HasUserVoted, opt => opt.Ignore()); // Set manually in service
        CreateMap<CreateProductQuestionDto, ProductQuestion>();

        // ProductAnswer mappings
        CreateMap<ProductAnswer, ProductAnswerDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
            .ForMember(dest => dest.HasUserVoted, opt => opt.Ignore()); // Set manually in service
        CreateMap<CreateProductAnswerDto, ProductAnswer>();

        // ProductTemplate mappings
        CreateMap<ProductTemplate, ProductTemplateDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .AfterMap((src, dest) =>
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.Specifications = !string.IsNullOrEmpty(src.Specifications)
                    ? JsonSerializer.Deserialize<Dictionary<string, string>>(src.Specifications)
                    : null;
                dest.Attributes = !string.IsNullOrEmpty(src.Attributes)
                    ? JsonSerializer.Deserialize<Dictionary<string, string>>(src.Attributes)
                    : null;
            });
        CreateMap<CreateProductTemplateDto, ProductTemplate>();
        CreateMap<UpdateProductTemplateDto, ProductTemplate>();

        // SizeGuide mappings
        CreateMap<SizeGuide, SizeGuideDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Entries, opt => opt.MapFrom(src => 
                src.Entries != null ? src.Entries.OrderBy(e => e.DisplayOrder).ToList() : new List<SizeGuideEntry>()));

        // SizeGuideEntry mappings
        CreateMap<SizeGuideEntry, SizeGuideEntryDto>()
            .AfterMap((src, dest) =>
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.AdditionalMeasurements = !string.IsNullOrEmpty(src.AdditionalMeasurements)
                    ? JsonSerializer.Deserialize<Dictionary<string, string>>(src.AdditionalMeasurements)
                    : null;
            });

        // ProductComparison mappings
        CreateMap<ProductComparison, ProductComparisonDto>()
            .ForMember(dest => dest.Products, opt => opt.Ignore()); // Set manually in service

        // Product → ComparisonProductDto mapping
        CreateMap<Product, ComparisonProductDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.MainImage, opt => opt.MapFrom(src => src.ImageUrl))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "N/A"))
            .ForMember(dest => dest.Specifications, opt => opt.Ignore()) // TODO: Map from product specifications
            .ForMember(dest => dest.Features, opt => opt.Ignore()) // TODO: Map from product features
            .ForMember(dest => dest.Rating, opt => opt.Ignore()) // Set manually in service
            .ForMember(dest => dest.ReviewCount, opt => opt.Ignore()) // Set manually in service
            .ForMember(dest => dest.Position, opt => opt.Ignore()); // Set manually in service

        // Search domain mappings
        // Product → ProductSuggestionDto mapping
        CreateMap<Product, ProductSuggestionDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.DiscountPrice ?? src.Price));

        // Product → ProductRecommendationDto mapping
        CreateMap<Product, ProductRecommendationDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.RecommendationReason, opt => opt.Ignore()) // Set manually in service
            .ForMember(dest => dest.RecommendationScore, opt => opt.Ignore()); // Set manually in service

        // Invoice mappings
        CreateMap<Invoice, InvoiceDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : string.Empty))
            .ForMember(dest => dest.BillingAddress, opt => opt.MapFrom(src => src.Order != null ? src.Order.Address : null))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Order != null && src.Order.OrderItems != null 
                ? src.Order.OrderItems.ToList() 
                : new List<OrderItem>()));
        
        // PaymentMethod mappings
        CreateMap<PaymentMethod, PaymentMethodDto>()
            .AfterMap((src, dest) =>
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.Settings = !string.IsNullOrEmpty(src.Settings)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Settings)
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

        // Warehouse mappings
        CreateMap<Warehouse, WarehouseDto>();
        CreateMap<CreateWarehouseDto, Warehouse>();
        CreateMap<UpdateWarehouseDto, Warehouse>();

        // Inventory mappings
        CreateMap<Inventory, InventoryDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product.SKU))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse.Name))
            .ForMember(dest => dest.WarehouseCode, opt => opt.MapFrom(src => src.Warehouse.Code))
            .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => src.AvailableQuantity));
        CreateMap<CreateInventoryDto, Inventory>();
        CreateMap<UpdateInventoryDto, Inventory>();

        // LowStockAlert mappings
        CreateMap<Inventory, LowStockAlertDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
            .ForMember(dest => dest.WarehouseId, opt => opt.MapFrom(src => src.WarehouseId))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            .ForMember(dest => dest.CurrentQuantity, opt => opt.MapFrom(src => src.Quantity))
            .ForMember(dest => dest.MinimumStockLevel, opt => opt.MapFrom(src => src.MinimumStockLevel))
            .ForMember(dest => dest.QuantityNeeded, opt => opt.MapFrom(src => src.MinimumStockLevel - src.Quantity));

        // StockMovement mappings
        CreateMap<StockMovement, StockMovementDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            .ForMember(dest => dest.MovementTypeName, opt => opt.MapFrom(src => src.MovementType.ToString()))
            .ForMember(dest => dest.PerformedByName, opt => opt.MapFrom(src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null))
            .ForMember(dest => dest.FromWarehouseName, opt => opt.MapFrom(src => src.FromWarehouse != null ? src.FromWarehouse.Name : null))
            .ForMember(dest => dest.ToWarehouseName, opt => opt.MapFrom(src => src.ToWarehouse != null ? src.ToWarehouse.Name : null));
        CreateMap<CreateStockMovementDto, StockMovement>();

        // DeliveryTimeEstimation mappings
        CreateMap<DeliveryTimeEstimation, DeliveryTimeEstimationDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null))
            .AfterMap((src, dest) =>
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.Conditions = !string.IsNullOrEmpty(src.Conditions)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Conditions!)
                    : null;
            });
        CreateMap<CreateDeliveryTimeEstimationDto, DeliveryTimeEstimation>();

        // PickPack mappings
        CreateMap<PickPack, PickPackDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.Order != null ? src.Order.OrderNumber : string.Empty))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            .ForMember(dest => dest.PickedByName, opt => opt.MapFrom(src => src.PickedBy != null ? $"{src.PickedBy.FirstName} {src.PickedBy.LastName}" : null))
            .ForMember(dest => dest.PackedByName, opt => opt.MapFrom(src => src.PackedBy != null ? $"{src.PackedBy.FirstName} {src.PackedBy.LastName}" : null));
        CreateMap<CreatePickPackDto, PickPack>();

        // PickPackItem mappings
        CreateMap<PickPackItem, PickPackItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductSKU, opt => opt.MapFrom(src => src.Product != null ? src.Product.SKU : string.Empty));

        // ShippingAddress mappings
        CreateMap<ShippingAddress, ShippingAddressDto>();
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
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : "Unknown"))
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<PreOrderCampaign, PreOrderCampaignDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : "Unknown"))
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : string.Empty))
            .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => 
                src.MaxQuantity > 0 ? src.MaxQuantity - src.CurrentQuantity : int.MaxValue))
            .ForMember(dest => dest.IsFull, opt => opt.MapFrom(src => 
                src.MaxQuantity > 0 && src.CurrentQuantity >= src.MaxQuantity));

        // AbandonedCartEmail mappings
        CreateMap<AbandonedCartEmail, AbandonedCartEmailDto>()
            .ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.Coupon != null ? src.Coupon.Code : null));

        // Cart -> AbandonedCartDto mapping (complex mapping with computed properties)
        CreateMap<Cart, AbandonedCartDto>()
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
        // Blog mappings
        CreateMap<BlogCategory, BlogCategoryDto>()
            .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.Name : null))
            .ForMember(dest => dest.PostCount, opt => opt.Ignore()) // Set manually (batch loading)
            .ForMember(dest => dest.SubCategories, opt => opt.Ignore()); // Set manually (recursive)

        CreateMap<BlogPost, BlogPostDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => 
                src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : string.Empty))
            .AfterMap((src, dest) => 
            {
                // ✅ FIX: Split().ToList() expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.Tags = !string.IsNullOrEmpty(src.Tags) ? src.Tags.Split(',').ToList() : new List<string>();
            });

        CreateMap<BlogComment, BlogCommentDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : src.AuthorName))
            .ForMember(dest => dest.ReplyCount, opt => opt.Ignore()) // Set manually (computed from Replies)
            .ForMember(dest => dest.Replies, opt => opt.Ignore()); // Set manually (recursive)

        // CMS mappings
        CreateMap<CMSPage, CMSPageDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => 
                src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : null))
            .ForMember(dest => dest.ParentPageTitle, opt => opt.MapFrom(src => src.ParentPage != null ? src.ParentPage.Title : null));

        // LandingPage mappings
        CreateMap<LandingPage, LandingPageDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => 
                src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : null));

        CreateMap<LandingPage, LandingPageVariantDto>()
            .ForMember(dest => dest.Views, opt => opt.MapFrom(src => src.ViewCount))
            .ForMember(dest => dest.Conversions, opt => opt.MapFrom(src => src.ConversionCount));

        // PageBuilder mappings
        CreateMap<PageBuilder, PageBuilderDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => 
                src.Author != null ? $"{src.Author.FirstName} {src.Author.LastName}" : null));

        // SEO mappings
        CreateMap<SEOSettings, SEOSettingsDto>()
            .AfterMap((src, dest) => 
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.StructuredData = !string.IsNullOrEmpty(src.StructuredData) 
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.StructuredData) 
                    : null;
            });

        CreateMap<SitemapEntry, SitemapEntryDto>();

        // Governance mappings
        CreateMap<Policy, PolicyDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => 
                src.CreatedBy != null ? $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}" : null));

        CreateMap<PolicyAcceptance, PolicyAcceptanceDto>()
            .ForMember(dest => dest.PolicyTitle, opt => opt.MapFrom(src => src.Policy != null ? src.Policy.Title : string.Empty))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty));

        // AuditLog mapping
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity.ToString()));

        // International mappings
        CreateMap<Language, LanguageDto>();

        CreateMap<Currency, CurrencyDto>();

        CreateMap<ProductTranslation, ProductTranslationDto>();

        CreateMap<CategoryTranslation, CategoryTranslationDto>();

        CreateMap<StaticTranslation, StaticTranslationDto>();

        // Identity mappings
        // User -> UserDto mapping zaten var (line 47)
        // TwoFactorAuth -> TwoFactorStatusDto için özel mapping (MaskPhoneNumber, MaskEmail, BackupCodesRemaining)
        CreateMap<TwoFactorAuth, TwoFactorStatusDto>()
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore()) // Set manually (MaskPhoneNumber)
            .ForMember(dest => dest.Email, opt => opt.Ignore()) // Set manually (MaskEmail)
            .ForMember(dest => dest.BackupCodesRemaining, opt => opt.Ignore()); // Set manually (Array.Length)

        // LiveCommerce mappings
        CreateMap<LiveStream, LiveStreamDto>()
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller != null ? src.Seller.StoreName : string.Empty))
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));

        CreateMap<LiveStreamProduct, LiveStreamProductDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductImageUrl, opt => opt.MapFrom(src => src.Product != null ? src.Product.ImageUrl : string.Empty))
            .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product != null ? src.Product.Price : 0));

        CreateMap<LiveStreamOrder, LiveStreamOrderDto>();

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
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .AfterMap((src, dest) =>
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.Variables = !string.IsNullOrEmpty(src.Variables)
                    ? JsonSerializer.Deserialize<List<string>>(src.Variables) ?? new()
                    : new();
            });
        CreateMap<CreateEmailTemplateDto, EmailTemplate>();

        CreateMap<EmailSubscriber, EmailSubscriberDto>()
            .AfterMap((src, dest) =>
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.Tags = !string.IsNullOrEmpty(src.Tags)
                    ? JsonSerializer.Deserialize<List<string>>(src.Tags) ?? new()
                    : new();
            });
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
        CreateMap<Organization, OrganizationDto>()
            .AfterMap((src, dest) =>
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.Settings = !string.IsNullOrEmpty(src.Settings)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Settings)
                    : null;
            });

        CreateMap<Team, TeamDto>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization != null ? src.Organization.Name : string.Empty))
            .ForMember(dest => dest.TeamLeadName, opt => opt.MapFrom(src => 
                src.TeamLead != null ? $"{src.TeamLead.FirstName} {src.TeamLead.LastName}" : null))
            .AfterMap((src, dest) =>
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.Settings = !string.IsNullOrEmpty(src.Settings)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Settings)
                    : null;
            });

        CreateMap<TeamMember, TeamMemberDto>()
            .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team != null ? src.Team.Name : string.Empty))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty));

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
                dest.Conditions = !string.IsNullOrEmpty(src.Conditions)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Conditions)
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

        // Notification mappings
        CreateMap<NotificationPreference, NotificationPreferenceDto>()
            .AfterMap((src, dest) =>
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.CustomSettings = !string.IsNullOrEmpty(src.CustomSettings)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.CustomSettings)
                    : null;
            });
        CreateMap<CreateNotificationPreferenceDto, NotificationPreference>();

        CreateMap<NotificationTemplate, NotificationTemplateDto>()
            .AfterMap((src, dest) =>
            {
                // ✅ FIX: JsonSerializer.Deserialize expression tree içinde kullanılamaz, AfterMap kullanıyoruz
                dest.Variables = !string.IsNullOrEmpty(src.Variables)
                    ? JsonSerializer.Deserialize<Dictionary<string, string>>(src.Variables)
                    : null;
                dest.DefaultData = !string.IsNullOrEmpty(src.DefaultData)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.DefaultData)
                    : null;
            });
        CreateMap<CreateNotificationTemplateDto, NotificationTemplate>();
        CreateMap<UpdateNotificationTemplateDto, NotificationTemplate>();

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
        CreateMap<OrderVerification, OrderVerificationDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => 
                src.Order != null ? src.Order.OrderNumber : string.Empty))
            .ForMember(dest => dest.VerifiedByName, opt => opt.MapFrom(src => 
                src.VerifiedBy != null ? $"{src.VerifiedBy.FirstName} {src.VerifiedBy.LastName}" : null));

        CreateMap<PaymentFraudPrevention, PaymentFraudPreventionDto>()
            .ForMember(dest => dest.PaymentTransactionId, opt => opt.MapFrom(src => 
                src.Payment != null ? src.Payment.TransactionId : string.Empty))
            .AfterMap((src, dest) =>
            {
                dest.CheckResult = !string.IsNullOrEmpty(src.CheckResult)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.CheckResult)
                    : null;
            });

        CreateMap<AccountSecurityEvent, AccountSecurityEventDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
            .ForMember(dest => dest.ActionTakenByName, opt => opt.MapFrom(src => 
                src.ActionTakenBy != null ? $"{src.ActionTakenBy.FirstName} {src.ActionTakenBy.LastName}" : null))
            .AfterMap((src, dest) =>
            {
                dest.Details = !string.IsNullOrEmpty(src.Details)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Details)
                    : null;
            });

        CreateMap<SecurityAlert, SecurityAlertDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
                src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null))
            .ForMember(dest => dest.AcknowledgedByName, opt => opt.MapFrom(src => 
                src.AcknowledgedBy != null ? $"{src.AcknowledgedBy.FirstName} {src.AcknowledgedBy.LastName}" : null))
            .ForMember(dest => dest.ResolvedByName, opt => opt.MapFrom(src => 
                src.ResolvedBy != null ? $"{src.ResolvedBy.FirstName} {src.ResolvedBy.LastName}" : null))
            .AfterMap((src, dest) =>
            {
                dest.Metadata = !string.IsNullOrEmpty(src.Metadata)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Metadata)
                    : null;
            });

        // Seller domain mappings
        CreateMap<SellerTransaction, SellerTransactionDto>();

        CreateMap<SellerInvoice, SellerInvoiceDto>()
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
                src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : string.Empty))
            .AfterMap((src, dest) =>
            {
                dest.Items = !string.IsNullOrEmpty(src.InvoiceData)
                    ? JsonSerializer.Deserialize<List<InvoiceItemDto>>(src.InvoiceData) ?? new List<InvoiceItemDto>()
                    : new List<InvoiceItemDto>();
            });

        CreateMap<SellerCommission, SellerCommissionDto>()
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
                src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : "Unknown"))
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => 
                src.Order != null ? src.Order.OrderNumber : "Unknown"))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<CommissionPayout, CommissionPayoutDto>()
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => 
                src.Seller != null ? $"{src.Seller.FirstName} {src.Seller.LastName}" : "Unknown"))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
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
                dest.Settings = !string.IsNullOrEmpty(src.Settings)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Settings)
                    : null;
            });

        // Subscription Domain Mappings
        CreateMap<SubscriptionPlan, SubscriptionPlanDto>()
            .ForMember(dest => dest.Features, opt => opt.Ignore()) // Will be set in AfterMap
            .ForMember(dest => dest.SubscriberCount, opt => opt.Ignore()) // Will be set in SubscriptionService after batch loading
            .AfterMap((src, dest) =>
            {
                dest.Features = !string.IsNullOrEmpty(src.Features)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Features)
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
                dest.Metadata = !string.IsNullOrEmpty(src.Metadata)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(src.Metadata)
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

