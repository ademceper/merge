using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.DTOs.Support;
using Merge.Application.DTOs.Marketing;
using Merge.Application.DTOs.Logistics;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Seller;
using Merge.Application.DTOs.B2B;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Content;
using Merge.Domain.Modules.Support;
using Merge.Domain.Modules.Payment;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Analytics;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketing;
using InventoryEntity = Merge.Domain.Modules.Inventory.Inventory;
using PreOrderCampaign = Merge.Domain.Modules.Marketing.PreOrderCampaign;
using AbandonedCartEmail = Merge.Domain.Modules.Marketing.AbandonedCartEmail;
using System.Text.Json;

namespace Merge.Application.Mappings.Content;

public class ContentMappingProfile : Profile
{
    public ContentMappingProfile()
    {
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


    }
}
