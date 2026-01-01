using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Merge.Domain.Entities;
using System.Linq.Expressions;
using System.Reflection;

namespace Merge.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderSplit> OrderSplits { get; set; }
    public DbSet<OrderSplitItem> OrderSplitItems { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Shipping> Shippings { get; set; }
    public DbSet<ReturnRequest> ReturnRequests { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<FlashSale> FlashSales { get; set; }
    public DbSet<FlashSaleProduct> FlashSaleProducts { get; set; }
    public DbSet<ProductBundle> ProductBundles { get; set; }
    public DbSet<BundleItem> BundleItems { get; set; }
    public DbSet<RecentlyViewedProduct> RecentlyViewedProducts { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<SellerProfile> SellerProfiles { get; set; }
    public DbSet<SavedCartItem> SavedCartItems { get; set; }
    public DbSet<EmailVerification> EmailVerifications { get; set; }
    public DbSet<FAQ> FAQs { get; set; }
    public DbSet<Banner> Banners { get; set; }
    public DbSet<GiftCard> GiftCards { get; set; }
    public DbSet<GiftCardTransaction> GiftCardTransactions { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<TwoFactorAuth> TwoFactorAuths { get; set; }
    public DbSet<TwoFactorCode> TwoFactorCodes { get; set; }
    public DbSet<SellerApplication> SellerApplications { get; set; }
    public DbSet<SellerDocument> SellerDocuments { get; set; }
    public DbSet<SearchHistory> SearchHistories { get; set; }
    public DbSet<PopularSearch> PopularSearches { get; set; }
    public DbSet<AbandonedCartEmail> AbandonedCartEmails { get; set; }
    public DbSet<UserActivityLog> UserActivityLogs { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<ExchangeRateHistory> ExchangeRateHistories { get; set; }
    public DbSet<UserCurrencyPreference> UserCurrencyPreferences { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<ProductTranslation> ProductTranslations { get; set; }
    public DbSet<CategoryTranslation> CategoryTranslations { get; set; }
    public DbSet<UserLanguagePreference> UserLanguagePreferences { get; set; }
    public DbSet<StaticTranslation> StaticTranslations { get; set; }
    public DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; }
    public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
    public DbSet<LoyaltyTier> LoyaltyTiers { get; set; }
    public DbSet<LoyaltyRule> LoyaltyRules { get; set; }
    public DbSet<ReferralCode> ReferralCodes { get; set; }
    public DbSet<Referral> Referrals { get; set; }
    public DbSet<ReviewMedia> ReviewMedias { get; set; }
    public DbSet<SharedWishlist> SharedWishlists { get; set; }
    public DbSet<SharedWishlistItem> SharedWishlistItems { get; set; }
    public DbSet<EmailCampaign> EmailCampaigns { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<EmailSubscriber> EmailSubscribers { get; set; }
    public DbSet<EmailCampaignRecipient> EmailCampaignRecipients { get; set; }
    public DbSet<EmailAutomation> EmailAutomations { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportSchedule> ReportSchedules { get; set; }
    public DbSet<DashboardMetric> DashboardMetrics { get; set; }
    public DbSet<ProductComparison> ProductComparisons { get; set; }
    public DbSet<ProductComparisonItem> ProductComparisonItems { get; set; }
    public DbSet<PreOrder> PreOrders { get; set; }
    public DbSet<PreOrderCampaign> PreOrderCampaigns { get; set; }
    public DbSet<SizeGuide> SizeGuides { get; set; }
    public DbSet<SizeGuideEntry> SizeGuideEntries { get; set; }
    public DbSet<ProductSizeGuide> ProductSizeGuides { get; set; }
    public DbSet<VirtualTryOn> VirtualTryOns { get; set; }
    public DbSet<ReviewHelpfulness> ReviewHelpfulnesses { get; set; }
    public DbSet<SupportTicket> SupportTickets { get; set; }
    public DbSet<TicketMessage> TicketMessages { get; set; }
    public DbSet<TicketAttachment> TicketAttachments { get; set; }
    public DbSet<ProductQuestion> ProductQuestions { get; set; }
    public DbSet<ProductAnswer> ProductAnswers { get; set; }
    public DbSet<QuestionHelpfulness> QuestionHelpfulnesses { get; set; }
    public DbSet<AnswerHelpfulness> AnswerHelpfulnesses { get; set; }
    public DbSet<SellerCommission> SellerCommissions { get; set; }
    public DbSet<CommissionTier> CommissionTiers { get; set; }
    public DbSet<SellerCommissionSettings> SellerCommissionSettings { get; set; }
    public DbSet<CommissionPayout> CommissionPayouts { get; set; }
    public DbSet<CommissionPayoutItem> CommissionPayoutItems { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }
    public DbSet<TrustBadge> TrustBadges { get; set; }
    public DbSet<SellerTrustBadge> SellerTrustBadges { get; set; }
    public DbSet<ProductTrustBadge> ProductTrustBadges { get; set; }
    public DbSet<KnowledgeBaseArticle> KnowledgeBaseArticles { get; set; }
    public DbSet<KnowledgeBaseCategory> KnowledgeBaseCategories { get; set; }
    public DbSet<KnowledgeBaseView> KnowledgeBaseViews { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<PolicyAcceptance> PolicyAcceptances { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }
    public DbSet<CustomerCommunication> CustomerCommunications { get; set; }
    public DbSet<SellerTransaction> SellerTransactions { get; set; }
    public DbSet<SellerInvoice> SellerInvoices { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
    public DbSet<SubscriptionUsage> SubscriptionUsages { get; set; }
    public DbSet<BlogCategory> BlogCategories { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<BlogComment> BlogComments { get; set; }
    public DbSet<BlogPostView> BlogPostViews { get; set; }
    public DbSet<SEOSettings> SEOSettings { get; set; }
    public DbSet<SitemapEntry> SitemapEntries { get; set; }
    public DbSet<CMSPage> CMSPages { get; set; }
    public DbSet<LandingPage> LandingPages { get; set; }
    public DbSet<LiveChatSession> LiveChatSessions { get; set; }
    public DbSet<LiveChatMessage> LiveChatMessages { get; set; }
    public DbSet<FraudDetectionRule> FraudDetectionRules { get; set; }
    public DbSet<FraudAlert> FraudAlerts { get; set; }
    public DbSet<OAuthProvider> OAuthProviders { get; set; }
    public DbSet<OAuthAccount> OAuthAccounts { get; set; }
    public DbSet<PushNotificationDevice> PushNotificationDevices { get; set; }
    public DbSet<LiveStream> LiveStreams { get; set; }
    public DbSet<LiveStreamProduct> LiveStreamProducts { get; set; }
    public DbSet<LiveStreamViewer> LiveStreamViewers { get; set; }
    public DbSet<LiveStreamOrder> LiveStreamOrders { get; set; }
    public DbSet<PageBuilder> PageBuilders { get; set; }
    public DbSet<DataWarehouse> DataWarehouses { get; set; }
    public DbSet<ETLProcess> ETLProcesses { get; set; }
    public DbSet<DataPipeline> DataPipelines { get; set; }
    public DbSet<DataQualityRule> DataQualityRules { get; set; }
    public DbSet<DataQualityCheck> DataQualityChecks { get; set; }
    public DbSet<PushNotification> PushNotifications { get; set; }
    public DbSet<InternationalShipping> InternationalShippings { get; set; }
    public DbSet<TaxRule> TaxRules { get; set; }
    public DbSet<CustomsDeclaration> CustomsDeclarations { get; set; }
    public DbSet<OrderVerification> OrderVerifications { get; set; }
    public DbSet<PaymentFraudPrevention> PaymentFraudPreventions { get; set; }
    public DbSet<AccountSecurityEvent> AccountSecurityEvents { get; set; }
    public DbSet<SecurityAlert> SecurityAlerts { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ✅ PERFORMANCE FIX: Global Query Filter for Soft Delete
        // Automatically filters out soft-deleted entities for all queries
        // This eliminates the need for manual "!IsDeleted" checks in 500+ locations
        ConfigureGlobalQueryFilters(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.PhoneNumber);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.ParentCategory)
                  .WithMany(e => e.SubCategories)
                  .HasForeignKey(e => e.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.SKU).IsUnique();
            // ✅ PERFORMANCE: Database Indexes (BOLUM 6.5)
            entity.HasIndex(e => e.SellerId);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.CategoryId, e.IsActive });
            
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPrice).HasPrecision(18, 2);
            
            // ✅ SECURITY: Check Constraints (BOLUM 7.3)
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Product_Price_Positive", "\"Price\" >= 0");
                t.HasCheckConstraint("CK_Product_Stock_NonNegative", "\"StockQuantity\" >= 0");
            });
            
            entity.HasOne(e => e.Category)
                  .WithMany(e => e.Products)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Seller)
                  .WithMany()
                  .HasForeignKey(e => e.SellerId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Store)
                  .WithMany(e => e.Products)
                  .HasForeignKey(e => e.StoreId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            // Store ImageUrls as JSON
            entity.Property(e => e.ImageUrls)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        });

        // Address configuration
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany(e => e.Addresses)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Cart configuration
        modelBuilder.Entity<Cart>(entity =>
        {
            // ✅ PERFORMANCE: Database Indexes
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                  .WithMany(e => e.Carts)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // CartItem configuration
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasOne(e => e.Cart)
                  .WithMany(e => e.CartItems)
                  .HasForeignKey(e => e.CartId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.CartItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            // ✅ PERFORMANCE: Database Indexes (BOLUM 6.5)
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.UserId, e.Status });
            
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.ShippingCost).HasPrecision(18, 2);
            entity.Property(e => e.Tax).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            
            // ✅ SECURITY: Check Constraints (BOLUM 7.3)
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Order_TotalAmount_Positive", "\"TotalAmount\" >= 0");
                t.HasCheckConstraint("CK_Order_SubTotal_Positive", "\"SubTotal\" >= 0");
                t.HasCheckConstraint("CK_Order_ShippingCost_Positive", "\"ShippingCost\" >= 0");
                t.HasCheckConstraint("CK_Order_Tax_Positive", "\"Tax\" >= 0");
            });
            
            entity.HasOne(e => e.User)
                  .WithMany(e => e.Orders)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Address)
                  .WithMany(e => e.Orders)
                  .HasForeignKey(e => e.AddressId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ParentOrder)
                  .WithMany(e => e.SplitOrders)
                  .HasForeignKey(e => e.ParentOrderId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasOne(e => e.Order)
                  .WithMany(e => e.OrderItems)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.OrderItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
        });

        // OrderSplit configuration
        modelBuilder.Entity<OrderSplit>(entity =>
        {
            entity.HasOne(e => e.OriginalOrder)
                  .WithMany(e => e.OriginalSplits)
                  .HasForeignKey(e => e.OriginalOrderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.SplitOrder)
                  .WithMany(e => e.SplitFrom)
                  .HasForeignKey(e => e.SplitOrderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.NewAddress)
                  .WithMany()
                  .HasForeignKey(e => e.NewAddressId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.SplitReason).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        // OrderSplitItem configuration
        modelBuilder.Entity<OrderSplitItem>(entity =>
        {
            entity.HasOne(e => e.OrderSplit)
                  .WithMany()
                  .HasForeignKey(e => e.OrderSplitId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.OriginalOrderItem)
                  .WithMany()
                  .HasForeignKey(e => e.OriginalOrderItemId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.SplitOrderItem)
                  .WithMany()
                  .HasForeignKey(e => e.SplitOrderItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Review configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany(e => e.Reviews)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.Reviews)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // ✅ PERFORMANCE: Database Indexes
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.ProductId });
            entity.HasIndex(e => new { e.ProductId, e.IsApproved });

            // ✅ SECURITY: Check Constraints (BOLUM 7.3)
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Review_Rating_Range", "\"Rating\" >= 1 AND \"Rating\" <= 5");
            });
        });

        // ProductVariant configuration
        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.Variants)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });

        // Wishlist configuration
        modelBuilder.Entity<Wishlist>(entity =>
        {
            // ✅ PERFORMANCE: Database Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();

            entity.HasOne(e => e.User)
                  .WithMany(e => e.Wishlists)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.Wishlists)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Coupon configuration
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            entity.Property(e => e.MinimumPurchaseAmount).HasPrecision(18, 2);
            entity.Property(e => e.MaximumDiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.ApplicableCategoryIds)
                  .HasConversion(
                      v => v != null ? string.Join(',', v) : null,
                      v => v != null ? v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList() : null);
            entity.Property(e => e.ApplicableProductIds)
                  .HasConversion(
                      v => v != null ? string.Join(',', v) : null,
                      v => v != null ? v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList() : null);
            
            // ✅ SECURITY: Check Constraints (BOLUM 7.3)
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Coupon_DiscountAmount_NonNegative", "\"DiscountAmount\" >= 0");
                t.HasCheckConstraint("CK_Coupon_DiscountPercentage_Range", "\"DiscountPercentage\" IS NULL OR (\"DiscountPercentage\" >= 0 AND \"DiscountPercentage\" <= 100)");
                t.HasCheckConstraint("CK_Coupon_UsedCount_LessThan_UsageLimit", "\"UsedCount\" <= \"UsageLimit\" OR \"UsageLimit\" = 0");
            });
        });

        // CouponUsage configuration
        modelBuilder.Entity<CouponUsage>(entity =>
        {
            entity.HasOne(e => e.Coupon)
                  .WithMany()
                  .HasForeignKey(e => e.CouponId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User)
                  .WithMany(e => e.CouponUsages)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Order)
                  .WithMany()
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasOne(e => e.Order)
                  .WithOne(e => e.Payment)
                  .HasForeignKey<Payment>(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);

            // ✅ PERFORMANCE: Database Indexes (BOLUM 6.5)
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => e.Status);

            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Metadata)
                  .HasConversion(
                      v => v ?? string.Empty,
                      v => string.IsNullOrEmpty(v) ? null : v);

            // ✅ SECURITY: Check Constraints (BOLUM 7.3)
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Payment_Amount_Positive", "\"Amount\" >= 0");
            });
        });

        // Shipping configuration
        modelBuilder.Entity<Shipping>(entity =>
        {
            entity.HasOne(e => e.Order)
                  .WithOne(e => e.Shipping)
                  .HasForeignKey<Shipping>(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // ✅ PERFORMANCE: Database Indexes (BOLUM 6.5)
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.TrackingNumber);
            entity.HasIndex(e => e.Status);
            
            entity.Property(e => e.ShippingCost).HasPrecision(18, 2);
            
            // ✅ SECURITY: Check Constraints (BOLUM 7.3)
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Shipping_ShippingCost_NonNegative", "\"ShippingCost\" >= 0");
            });
        });

        // ReturnRequest configuration
        modelBuilder.Entity<ReturnRequest>(entity =>
        {
            entity.HasOne(e => e.Order)
                  .WithMany(e => e.ReturnRequests)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User)
                  .WithMany(e => e.ReturnRequests)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // ✅ PERFORMANCE: Database Indexes (BOLUM 6.5)
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            
            entity.Property(e => e.RefundAmount).HasPrecision(18, 2);
            entity.Property(e => e.OrderItemIds)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList());
            
            // ✅ SECURITY: Check Constraints (BOLUM 7.3)
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_ReturnRequest_RefundAmount_NonNegative", "\"RefundAmount\" >= 0");
            });
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany(e => e.Notifications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Data)
                  .HasConversion(
                      v => v ?? string.Empty,
                      v => string.IsNullOrEmpty(v) ? null : v);
            entity.HasIndex(e => new { e.UserId, e.IsRead });
        });

        // NotificationTemplate configuration
        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TitleTemplate).IsRequired().HasMaxLength(500);
            entity.Property(e => e.MessageTemplate).IsRequired();
            entity.HasIndex(e => e.Type);
        });


        // CartItem - ProductVariant relationship
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasOne(e => e.ProductVariant)
                  .WithMany()
                  .HasForeignKey(e => e.ProductVariantId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // FlashSale configuration
        modelBuilder.Entity<FlashSale>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
        });

        // FlashSaleProduct configuration
        modelBuilder.Entity<FlashSaleProduct>(entity =>
        {
            entity.HasOne(e => e.FlashSale)
                  .WithMany(e => e.FlashSaleProducts)
                  .HasForeignKey(e => e.FlashSaleId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.FlashSaleProducts)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.SalePrice).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.FlashSaleId, e.ProductId });
        });

        // ProductBundle configuration
        modelBuilder.Entity<ProductBundle>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.BundlePrice).HasPrecision(18, 2);
            entity.Property(e => e.OriginalTotalPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
        });

        // BundleItem configuration
        modelBuilder.Entity<BundleItem>(entity =>
        {
            entity.HasOne(e => e.Bundle)
                  .WithMany(e => e.BundleItems)
                  .HasForeignKey(e => e.BundleId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.BundleItems)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // RecentlyViewedProduct configuration
        modelBuilder.Entity<RecentlyViewedProduct>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany(e => e.RecentlyViewedProducts)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.RecentlyViewedProducts)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.ProductId, e.ViewedAt });
        });

        // Invoice configuration
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasOne(e => e.Order)
                  .WithOne(e => e.Invoice)
                  .HasForeignKey<Invoice>(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // ✅ PERFORMANCE: Database Indexes (BOLUM 6.5)
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.InvoiceDate);
            
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.Tax).HasPrecision(18, 2);
            entity.Property(e => e.ShippingCost).HasPrecision(18, 2);
            entity.Property(e => e.Discount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            
            // ✅ SECURITY: Check Constraints (BOLUM 7.3)
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Invoice_SubTotal_NonNegative", "\"SubTotal\" >= 0");
                t.HasCheckConstraint("CK_Invoice_Tax_NonNegative", "\"Tax\" >= 0");
                t.HasCheckConstraint("CK_Invoice_ShippingCost_NonNegative", "\"ShippingCost\" >= 0");
                t.HasCheckConstraint("CK_Invoice_Discount_NonNegative", "\"Discount\" >= 0");
                t.HasCheckConstraint("CK_Invoice_TotalAmount_NonNegative", "\"TotalAmount\" >= 0");
            });
        });

        // SellerProfile configuration
        modelBuilder.Entity<SellerProfile>(entity =>
        {
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne(e => e.User)
                  .WithOne(e => e.SellerProfile)
                  .HasForeignKey<SellerProfile>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // ✅ PERFORMANCE: Database Indexes (BOLUM 6.5)
            entity.HasIndex(e => e.Status);
            
            entity.Property(e => e.CommissionRate).HasPrecision(5, 2);
            entity.Property(e => e.TotalEarnings).HasPrecision(18, 2);
            entity.Property(e => e.PendingBalance).HasPrecision(18, 2);
            entity.Property(e => e.AvailableBalance).HasPrecision(18, 2);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
            
            // ✅ SECURITY: Check Constraints (BOLUM 7.3)
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_SellerProfile_CommissionRate_Range", "\"CommissionRate\" >= 0 AND \"CommissionRate\" <= 100");
                t.HasCheckConstraint("CK_SellerProfile_TotalEarnings_NonNegative", "\"TotalEarnings\" >= 0");
                t.HasCheckConstraint("CK_SellerProfile_PendingBalance_NonNegative", "\"PendingBalance\" >= 0");
                t.HasCheckConstraint("CK_SellerProfile_AvailableBalance_NonNegative", "\"AvailableBalance\" >= 0");
                t.HasCheckConstraint("CK_SellerProfile_AverageRating_Range", "\"AverageRating\" >= 0 AND \"AverageRating\" <= 5");
            });
        });

        // SavedCartItem configuration
        modelBuilder.Entity<SavedCartItem>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany(e => e.SavedCartItems)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.UserId, e.ProductId });
        });

        // EmailVerification configuration
        modelBuilder.Entity<EmailVerification>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany(e => e.EmailVerifications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // FAQ configuration
        modelBuilder.Entity<FAQ>(entity =>
        {
            entity.Property(e => e.Question).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Answer).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50);
        });

        // Banner configuration
        modelBuilder.Entity<Banner>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // GiftCard configuration
        modelBuilder.Entity<GiftCard>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.PurchasedBy)
                  .WithMany(e => e.PurchasedGiftCards)
                  .HasForeignKey(e => e.PurchasedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.AssignedTo)
                  .WithMany(e => e.AssignedGiftCards)
                  .HasForeignKey(e => e.AssignedToUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.RemainingAmount).HasPrecision(18, 2);

            // ✅ SECURITY: Check Constraints
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_GiftCard_Amount_Positive", "\"Amount\" > 0");
                t.HasCheckConstraint("CK_GiftCard_RemainingAmount_NonNegative", "\"RemainingAmount\" >= 0");
            });
        });

        // GiftCardTransaction configuration
        modelBuilder.Entity<GiftCardTransaction>(entity =>
        {
            entity.HasOne(e => e.GiftCard)
                  .WithMany()
                  .HasForeignKey(e => e.GiftCardId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Order)
                  .WithMany(e => e.GiftCardTransactions)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
        });

        // Warehouse configuration
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
        });

        // Inventory configuration
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Warehouse)
                  .WithMany(e => e.Inventories)
                  .HasForeignKey(e => e.WarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.UnitCost).HasPrecision(18, 2);
            entity.HasIndex(e => new { e.ProductId, e.WarehouseId }).IsUnique();
            entity.Ignore(e => e.AvailableQuantity); // Computed property
        });

        // StockMovement configuration
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasOne(e => e.Inventory)
                  .WithMany(e => e.StockMovements)
                  .HasForeignKey(e => e.InventoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Warehouse)
                  .WithMany(e => e.StockMovements)
                  .HasForeignKey(e => e.WarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.PerformedBy)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.FromWarehouse)
                  .WithMany()
                  .HasForeignKey(e => e.FromWarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ToWarehouse)
                  .WithMany()
                  .HasForeignKey(e => e.ToWarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.ProductId, e.WarehouseId, e.CreatedAt });
        });

        // TwoFactorAuth configuration
        modelBuilder.Entity<TwoFactorAuth>(entity =>
        {
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne(e => e.User)
                  .WithOne()
                  .HasForeignKey<TwoFactorAuth>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.BackupCodes)
                  .HasConversion(
                      v => v != null ? string.Join(',', v) : null,
                      v => v != null ? v.Split(',', StringSplitOptions.RemoveEmptyEntries) : null);
        });

        // TwoFactorCode configuration
        modelBuilder.Entity<TwoFactorCode>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.Code, e.IsUsed });
            entity.HasIndex(e => e.ExpiresAt);
        });

        // SellerApplication configuration
        modelBuilder.Entity<SellerApplication>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Reviewer)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewedBy)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.BusinessName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EstimatedMonthlyRevenue).HasPrecision(18, 2);
            entity.HasIndex(e => e.Status);
        });

        // SellerDocument configuration
        modelBuilder.Entity<SellerDocument>(entity =>
        {
            entity.HasOne(e => e.SellerApplication)
                  .WithMany()
                  .HasForeignKey(e => e.SellerApplicationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // SearchHistory configuration
        modelBuilder.Entity<SearchHistory>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ClickedProduct)
                  .WithMany()
                  .HasForeignKey(e => e.ClickedProductId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.SearchTerm).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => new { e.SearchTerm, e.CreatedAt });
            entity.HasIndex(e => e.UserId);
        });

        // PopularSearch configuration
        modelBuilder.Entity<PopularSearch>(entity =>
        {
            entity.HasIndex(e => e.SearchTerm).IsUnique();
            entity.Property(e => e.SearchTerm).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ClickThroughRate).HasPrecision(5, 2);
            entity.HasIndex(e => new { e.SearchCount, e.LastSearchedAt });
        });

        // AbandonedCartEmail configuration
        modelBuilder.Entity<AbandonedCartEmail>(entity =>
        {
            entity.HasOne(e => e.Cart)
                  .WithMany()
                  .HasForeignKey(e => e.CartId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Coupon)
                  .WithMany()
                  .HasForeignKey(e => e.CouponId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.EmailType).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.CartId, e.SentAt });
            entity.HasIndex(e => new { e.UserId, e.SentAt });
        });

        // UserActivityLog configuration
        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.DeviceType).HasMaxLength(50);
            entity.Property(e => e.Browser).HasMaxLength(50);
            entity.Property(e => e.OS).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.ActivityType, e.CreatedAt });
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.TableName).HasMaxLength(100);
            entity.Property(e => e.PrimaryKey).HasMaxLength(100);
            entity.Property(e => e.Changes).HasMaxLength(2000);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Module).HasMaxLength(100);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.Action, e.CreatedAt });
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => new { e.Severity, e.CreatedAt });
            entity.HasIndex(e => e.Module);
        });

        // Currency configuration
        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
            entity.Property(e => e.ExchangeRate).HasPrecision(18, 6);
            entity.Property(e => e.Format).HasMaxLength(50);
            entity.HasIndex(e => e.IsBaseCurrency);
            entity.HasIndex(e => e.IsActive);
        });

        // ExchangeRateHistory configuration
        modelBuilder.Entity<ExchangeRateHistory>(entity =>
        {
            entity.HasOne(e => e.Currency)
                  .WithMany()
                  .HasForeignKey(e => e.CurrencyId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.CurrencyCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.ExchangeRate).HasPrecision(18, 6);
            entity.Property(e => e.Source).HasMaxLength(50);
            entity.HasIndex(e => new { e.CurrencyId, e.RecordedAt });
        });

        // UserCurrencyPreference configuration
        modelBuilder.Entity<UserCurrencyPreference>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Currency)
                  .WithMany()
                  .HasForeignKey(e => e.CurrencyId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.CurrencyCode).IsRequired().HasMaxLength(10);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Language configuration
        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NativeName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FlagIcon).HasMaxLength(200);
            entity.HasIndex(e => e.IsDefault);
            entity.HasIndex(e => e.IsActive);
        });

        // ProductTranslation configuration
        modelBuilder.Entity<ProductTranslation>(entity =>
        {
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.LanguageCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => new { e.ProductId, e.LanguageCode }).IsUnique();
        });

        // CategoryTranslation configuration
        modelBuilder.Entity<CategoryTranslation>(entity =>
        {
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.LanguageCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => new { e.CategoryId, e.LanguageCode }).IsUnique();
        });

        // UserLanguagePreference configuration
        modelBuilder.Entity<UserLanguagePreference>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.LanguageCode).IsRequired().HasMaxLength(10);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // StaticTranslation configuration
        modelBuilder.Entity<StaticTranslation>(entity =>
        {
            entity.HasOne(e => e.Language)
                  .WithMany()
                  .HasForeignKey(e => e.LanguageId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(200);
            entity.Property(e => e.LanguageCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Value).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.HasIndex(e => new { e.Key, e.LanguageCode }).IsUnique();
        });

        // LoyaltyAccount configuration
        modelBuilder.Entity<LoyaltyAccount>(entity =>
        {
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tier).WithMany().HasForeignKey(e => e.TierId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // LoyaltyTransaction configuration
        modelBuilder.Entity<LoyaltyTransaction>(entity =>
        {
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.LoyaltyAccount).WithMany().HasForeignKey(e => e.LoyaltyAccountId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        });

        // LoyaltyTier configuration
        modelBuilder.Entity<LoyaltyTier>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            entity.Property(e => e.PointsMultiplier).HasPrecision(5, 2);
            entity.HasIndex(e => e.Level).IsUnique();
        });

        // LoyaltyRule configuration
        modelBuilder.Entity<LoyaltyRule>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MinimumPurchaseAmount).HasPrecision(18, 2);
        });

        // ReferralCode configuration
        modelBuilder.Entity<ReferralCode>(entity =>
        {
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
        });

        // Referral configuration
        modelBuilder.Entity<Referral>(entity =>
        {
            entity.HasOne(e => e.Referrer).WithMany().HasForeignKey(e => e.ReferrerId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ReferredUser).WithMany().HasForeignKey(e => e.ReferredUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ReferralCodeEntity).WithMany().HasForeignKey(e => e.ReferralCodeId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.ReferralCode).IsRequired().HasMaxLength(20);
        });

        // ReviewMedia configuration
        modelBuilder.Entity<ReviewMedia>(entity =>
        {
            entity.HasOne(e => e.Review).WithMany().HasForeignKey(e => e.ReviewId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
        });

        // SharedWishlist configuration
        modelBuilder.Entity<SharedWishlist>(entity =>
        {
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.ShareCode).IsUnique();
            entity.Property(e => e.ShareCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });

        // SharedWishlistItem configuration
        modelBuilder.Entity<SharedWishlistItem>(entity =>
        {
            entity.HasOne(e => e.SharedWishlist).WithMany().HasForeignKey(e => e.SharedWishlistId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PurchasedByUser).WithMany().HasForeignKey(e => e.PurchasedBy).OnDelete(DeleteBehavior.SetNull);
        });

        // EmailCampaign configuration
        modelBuilder.Entity<EmailCampaign>(entity =>
        {
            entity.HasOne(e => e.Template).WithMany(t => t.Campaigns).HasForeignKey(e => e.TemplateId).OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(300);
            entity.Property(e => e.FromName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FromEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ReplyToEmail).HasMaxLength(255);
            entity.Property(e => e.TargetSegment).HasMaxLength(50);
            entity.Property(e => e.OpenRate).HasPrecision(5, 2);
            entity.Property(e => e.ClickRate).HasPrecision(5, 2);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ScheduledAt);
            entity.HasIndex(e => e.SentAt);
        });

        // EmailTemplate configuration
        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Thumbnail).HasMaxLength(500);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
        });

        // EmailSubscriber configuration
        modelBuilder.Entity<EmailSubscriber>(entity =>
        {
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.HasIndex(e => e.IsSubscribed);
            entity.HasIndex(e => e.SubscribedAt);
        });

        // EmailCampaignRecipient configuration
        modelBuilder.Entity<EmailCampaignRecipient>(entity =>
        {
            entity.HasOne(e => e.Campaign).WithMany(c => c.Recipients).HasForeignKey(e => e.CampaignId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Subscriber).WithMany().HasForeignKey(e => e.SubscriberId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            entity.HasIndex(e => new { e.CampaignId, e.SubscriberId }).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // EmailAutomation configuration
        modelBuilder.Entity<EmailAutomation>(entity =>
        {
            entity.HasOne(e => e.Template).WithMany().HasForeignKey(e => e.TemplateId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
        });

        // Report configuration
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasOne(e => e.GeneratedByUser).WithMany().HasForeignKey(e => e.GeneratedBy).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.GeneratedBy, e.CreatedAt });
        });

        // ReportSchedule configuration
        modelBuilder.Entity<ReportSchedule>(entity =>
        {
            entity.HasOne(e => e.Owner).WithMany().HasForeignKey(e => e.OwnerId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.EmailRecipients).HasMaxLength(500);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.NextRunAt);
        });

        // DashboardMetric configuration
        modelBuilder.Entity<DashboardMetric>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ValueFormatted).HasMaxLength(100);
            entity.Property(e => e.Value).HasPrecision(18, 2);
            entity.Property(e => e.PreviousValue).HasPrecision(18, 2);
            entity.Property(e => e.ChangePercentage).HasPrecision(5, 2);
            entity.HasIndex(e => e.Key);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.CalculatedAt);
        });

        // ProductComparison configuration
        modelBuilder.Entity<ProductComparison>(entity =>
        {
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ShareCode).HasMaxLength(20);
            entity.HasIndex(e => e.ShareCode).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.IsSaved });
            entity.HasIndex(e => e.CreatedAt);
        });

        // ProductComparisonItem configuration
        modelBuilder.Entity<ProductComparisonItem>(entity =>
        {
            entity.HasOne(e => e.Comparison).WithMany(c => c.Items).HasForeignKey(e => e.ComparisonId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.ComparisonId, e.ProductId });
            entity.HasIndex(e => e.Position);
        });

        // PreOrder configuration
        modelBuilder.Entity<PreOrder>(entity =>
        {
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.DepositAmount).HasPrecision(18, 2);
            entity.Property(e => e.DepositPaid).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => new { e.UserId, e.Status });
            entity.HasIndex(e => e.ExpectedAvailabilityDate);
            entity.HasIndex(e => e.ExpiresAt);
        });

        // PreOrderCampaign configuration
        modelBuilder.Entity<PreOrderCampaign>(entity =>
        {
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SpecialPrice).HasPrecision(18, 2);
            entity.Property(e => e.DepositPercentage).HasPrecision(5, 2);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => new { e.StartDate, e.EndDate });
        });

        // SizeGuide configuration
        modelBuilder.Entity<SizeGuide>(entity =>
        {
            entity.HasOne(e => e.Category).WithMany().HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.MeasurementUnit).IsRequired().HasMaxLength(10);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.Brand);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Type);
        });

        // SizeGuideEntry configuration
        modelBuilder.Entity<SizeGuideEntry>(entity =>
        {
            entity.HasOne(e => e.SizeGuide).WithMany(sg => sg.Entries).HasForeignKey(e => e.SizeGuideId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.SizeLabel).IsRequired().HasMaxLength(50);
            entity.Property(e => e.AlternativeLabel).HasMaxLength(50);
            entity.Property(e => e.Chest).HasPrecision(10, 2);
            entity.Property(e => e.Waist).HasPrecision(10, 2);
            entity.Property(e => e.Hips).HasPrecision(10, 2);
            entity.Property(e => e.Inseam).HasPrecision(10, 2);
            entity.Property(e => e.Shoulder).HasPrecision(10, 2);
            entity.Property(e => e.Length).HasPrecision(10, 2);
            entity.Property(e => e.Width).HasPrecision(10, 2);
            entity.Property(e => e.Height).HasPrecision(10, 2);
            entity.Property(e => e.Weight).HasPrecision(10, 2);
            entity.Property(e => e.AdditionalMeasurements).HasColumnType("jsonb");
            entity.HasIndex(e => e.SizeGuideId);
            entity.HasIndex(e => e.DisplayOrder);
        });

        // ProductSizeGuide configuration
        modelBuilder.Entity<ProductSizeGuide>(entity =>
        {
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.SizeGuide).WithMany().HasForeignKey(e => e.SizeGuideId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.CustomNotes).HasMaxLength(1000);
            entity.Property(e => e.FitDescription).HasMaxLength(500);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.SizeGuideId);
        });

        // VirtualTryOn configuration
        modelBuilder.Entity<VirtualTryOn>(entity =>
        {
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.ModelUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PreviewImageUrl).HasMaxLength(500);
            entity.Property(e => e.Height).HasPrecision(10, 2);
            entity.Property(e => e.Chest).HasPrecision(10, 2);
            entity.Property(e => e.Waist).HasPrecision(10, 2);
            entity.Property(e => e.Hips).HasPrecision(10, 2);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
        });

        // ReviewHelpfulness configuration
        modelBuilder.Entity<ReviewHelpfulness>(entity =>
        {
            entity.HasOne(e => e.Review).WithMany(r => r.HelpfulnessVotes).HasForeignKey(e => e.ReviewId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.ReviewId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.ReviewId);
            entity.HasIndex(e => e.UserId);
        });

        // SupportTicket configuration
        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.AssignedTo).WithMany().HasForeignKey(e => e.AssignedToId).OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.TicketNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.TicketNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => new { e.UserId, e.Status });
            entity.HasIndex(e => e.AssignedToId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // TicketMessage configuration
        modelBuilder.Entity<TicketMessage>(entity =>
        {
            entity.HasOne(e => e.Ticket).WithMany(t => t.Messages).HasForeignKey(e => e.TicketId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Message).IsRequired();
            entity.HasIndex(e => e.TicketId);
            entity.HasIndex(e => new { e.TicketId, e.CreatedAt });
        });

        // TicketAttachment configuration
        modelBuilder.Entity<TicketAttachment>(entity =>
        {
            entity.HasOne(e => e.Ticket).WithMany(t => t.Attachments).HasForeignKey(e => e.TicketId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Message).WithMany(m => m.Attachments).HasForeignKey(e => e.MessageId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.HasIndex(e => e.TicketId);
            entity.HasIndex(e => e.MessageId);
        });

        // ProductQuestion configuration
        modelBuilder.Entity<ProductQuestion>(entity =>
        {
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Question).IsRequired();
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => new { e.ProductId, e.IsApproved });
            entity.HasIndex(e => e.CreatedAt);
        });

        // ProductAnswer configuration
        modelBuilder.Entity<ProductAnswer>(entity =>
        {
            entity.HasOne(e => e.Question).WithMany(q => q.Answers).HasForeignKey(e => e.QuestionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Answer).IsRequired();
            entity.HasIndex(e => e.QuestionId);
            entity.HasIndex(e => new { e.QuestionId, e.IsApproved });
            entity.HasIndex(e => e.CreatedAt);
        });

        // QuestionHelpfulness configuration
        modelBuilder.Entity<QuestionHelpfulness>(entity =>
        {
            entity.HasOne(e => e.Question).WithMany(q => q.HelpfulnessVotes).HasForeignKey(e => e.QuestionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.QuestionId, e.UserId }).IsUnique();
        });

        // AnswerHelpfulness configuration
        modelBuilder.Entity<AnswerHelpfulness>(entity =>
        {
            entity.HasOne(e => e.Answer).WithMany(a => a.HelpfulnessVotes).HasForeignKey(e => e.AnswerId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.AnswerId, e.UserId }).IsUnique();
        });

        // SellerCommission configuration
        modelBuilder.Entity<SellerCommission>(entity =>
        {
            entity.HasOne(e => e.Seller).WithMany().HasForeignKey(e => e.SellerId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.OrderItem).WithMany().HasForeignKey(e => e.OrderItemId).OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.OrderAmount).HasPrecision(18, 2);
            entity.Property(e => e.CommissionRate).HasPrecision(5, 2);
            entity.Property(e => e.CommissionAmount).HasPrecision(18, 2);
            entity.Property(e => e.PlatformFee).HasPrecision(18, 2);
            entity.Property(e => e.NetAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaymentReference).HasMaxLength(100);
            entity.HasIndex(e => e.SellerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.OrderId, e.OrderItemId });
        });

        // CommissionTier configuration
        modelBuilder.Entity<CommissionTier>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MinSales).HasPrecision(18, 2);
            entity.Property(e => e.MaxSales).HasPrecision(18, 2);
            entity.Property(e => e.CommissionRate).HasPrecision(5, 2);
            entity.Property(e => e.PlatformFeeRate).HasPrecision(5, 2);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Priority);
        });

        // SellerCommissionSettings configuration
        modelBuilder.Entity<SellerCommissionSettings>(entity =>
        {
            entity.HasOne(e => e.Seller).WithMany().HasForeignKey(e => e.SellerId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.CustomCommissionRate).HasPrecision(5, 2);
            entity.Property(e => e.MinimumPayoutAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.HasIndex(e => e.SellerId).IsUnique();
        });

        // CommissionPayout configuration
        modelBuilder.Entity<CommissionPayout>(entity =>
        {
            entity.HasOne(e => e.Seller).WithMany().HasForeignKey(e => e.SellerId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.PayoutNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.TransactionFee).HasPrecision(18, 2);
            entity.Property(e => e.NetAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TransactionReference).HasMaxLength(200);
            
            // ✅ PERFORMANCE: Database Indexes (BOLUM 6.5)
            entity.HasIndex(e => e.PayoutNumber).IsUnique();
            entity.HasIndex(e => e.SellerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PayoutDate);
            
            // ✅ SECURITY: Check Constraints (BOLUM 7.3)
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_CommissionPayout_TotalAmount_NonNegative", "\"TotalAmount\" >= 0");
                t.HasCheckConstraint("CK_CommissionPayout_TransactionFee_NonNegative", "\"TransactionFee\" >= 0");
                t.HasCheckConstraint("CK_CommissionPayout_NetAmount_NonNegative", "\"NetAmount\" >= 0");
            });
        });

        // CommissionPayoutItem configuration
        modelBuilder.Entity<CommissionPayoutItem>(entity =>
        {
            entity.HasOne(e => e.Payout).WithMany(p => p.Items).HasForeignKey(e => e.PayoutId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Commission).WithMany().HasForeignKey(e => e.CommissionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.PayoutId, e.CommissionId });
        });

        // UserPreference configuration
        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Theme).HasMaxLength(20);
            entity.Property(e => e.DefaultLanguage).HasMaxLength(10);
            entity.Property(e => e.DefaultCurrency).HasMaxLength(10);
            entity.Property(e => e.DateFormat).HasMaxLength(20);
            entity.Property(e => e.TimeFormat).HasMaxLength(10);
            entity.Property(e => e.DefaultShippingAddress).HasMaxLength(100);
            entity.Property(e => e.DefaultPaymentMethod).HasMaxLength(100);
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // TrustBadge configuration
        modelBuilder.Entity<TrustBadge>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.BadgeType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IconUrl).HasMaxLength(500);
            entity.Property(e => e.Color).HasMaxLength(10);
            entity.HasIndex(e => e.BadgeType);
        });

        // SellerTrustBadge configuration
        modelBuilder.Entity<SellerTrustBadge>(entity =>
        {
            entity.HasOne(e => e.Seller)
                  .WithMany()
                  .HasForeignKey(e => e.SellerId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.TrustBadge)
                  .WithMany()
                  .HasForeignKey(e => e.TrustBadgeId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.SellerId, e.TrustBadgeId });
        });

        // ProductTrustBadge configuration
        modelBuilder.Entity<ProductTrustBadge>(entity =>
        {
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.TrustBadge)
                  .WithMany()
                  .HasForeignKey(e => e.TrustBadgeId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.ProductId, e.TrustBadgeId });
        });

        // KnowledgeBaseCategory configuration
        modelBuilder.Entity<KnowledgeBaseCategory>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.ParentCategory)
                  .WithMany(e => e.SubCategories)
                  .HasForeignKey(e => e.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // KnowledgeBaseArticle configuration
        modelBuilder.Entity<KnowledgeBaseArticle>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.Category)
                  .WithMany(e => e.Articles)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Author)
                  .WithMany()
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CategoryId);
        });

        // KnowledgeBaseView configuration
        modelBuilder.Entity<KnowledgeBaseView>(entity =>
        {
            entity.HasOne(e => e.Article)
                  .WithMany(e => e.Views)
                  .HasForeignKey(e => e.ArticleId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.ArticleId, e.CreatedAt });
        });

        // NotificationPreference configuration
        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.NotificationType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Channel).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.UserId, e.NotificationType, e.Channel }).IsUnique();
        });

        // CustomerCommunication configuration
        modelBuilder.Entity<CustomerCommunication>(entity =>
        {
            entity.Property(e => e.CommunicationType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Channel).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Direction).HasMaxLength(20);
            entity.Property(e => e.RelatedEntityType).HasMaxLength(50);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.SentBy)
                  .WithMany()
                  .HasForeignKey(e => e.SentByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            // ✅ PERFORMANCE: Database Indexes (BOLUM 6.5)
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => e.CommunicationType);
            entity.HasIndex(e => e.Channel);
            entity.HasIndex(e => new { e.Status, e.CommunicationType });
        });

        // SellerTransaction configuration
        modelBuilder.Entity<SellerTransaction>(entity =>
        {
            entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.RelatedEntityType).HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceBefore).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAfter).HasPrecision(18, 2);
            entity.HasOne(e => e.Seller)
                  .WithMany()
                  .HasForeignKey(e => e.SellerId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.SellerId, e.CreatedAt });
            entity.HasIndex(e => e.TransactionType);
        });

        // SellerInvoice configuration
        modelBuilder.Entity<SellerInvoice>(entity =>
        {
            entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalEarnings).HasPrecision(18, 2);
            entity.Property(e => e.TotalCommissions).HasPrecision(18, 2);
            entity.Property(e => e.TotalPayouts).HasPrecision(18, 2);
            entity.Property(e => e.PlatformFees).HasPrecision(18, 2);
            entity.Property(e => e.NetAmount).HasPrecision(18, 2);
            entity.HasOne(e => e.Seller)
                  .WithMany()
                  .HasForeignKey(e => e.SellerId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.SellerId, e.InvoiceDate });
        });

        // Store configuration
        modelBuilder.Entity<Store>(entity =>
        {
            entity.Property(e => e.StoreName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.Seller)
                  .WithMany()
                  .HasForeignKey(e => e.SellerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // ✅ PERFORMANCE: Database Indexes (BOLUM 6.5)
            entity.HasIndex(e => e.SellerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.SellerId, e.IsPrimary });
            entity.HasIndex(e => new { e.Status, e.IsVerified });
        });

        // ProductTemplate configuration
        modelBuilder.Entity<ProductTemplate>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DefaultSKUPrefix).HasMaxLength(50);
            entity.Property(e => e.DefaultPrice).HasPrecision(18, 2);
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.IsActive);
        });

        // ShippingAddress configuration
        modelBuilder.Entity<ShippingAddress>(entity =>
        {
            entity.Property(e => e.Label).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.AddressLine1).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AddressLine2).HasMaxLength(200);
            entity.Property(e => e.City).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.IsDefault });
        });

        // PickPack configuration
        modelBuilder.Entity<PickPack>(entity =>
        {
            entity.Property(e => e.PackNumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.PackNumber).IsUnique();
            entity.Property(e => e.Dimensions).HasMaxLength(50);
            entity.Property(e => e.Weight).HasPrecision(10, 2);
            entity.HasOne(e => e.Order)
                  .WithMany()
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Warehouse)
                  .WithMany()
                  .HasForeignKey(e => e.WarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.PickedBy)
                  .WithMany()
                  .HasForeignKey(e => e.PickedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.PackedBy)
                  .WithMany()
                  .HasForeignKey(e => e.PackedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            
            // ✅ PERFORMANCE: Database Indexes (BOLUM 6.5)
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.WarehouseId);
            entity.HasIndex(e => new { e.OrderId, e.Status });
            
            // ✅ SECURITY: Check Constraints (BOLUM 7.3)
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_PickPack_Weight_NonNegative", "\"Weight\" >= 0");
                t.HasCheckConstraint("CK_PickPack_PackageCount_Positive", "\"PackageCount\" > 0");
            });
        });

        // PickPackItem configuration
        modelBuilder.Entity<PickPackItem>(entity =>
        {
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.HasOne(e => e.PickPack)
                  .WithMany(e => e.Items)
                  .HasForeignKey(e => e.PickPackId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.OrderItem)
                  .WithMany()
                  .HasForeignKey(e => e.OrderItemId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.PickPackId, e.OrderItemId });
        });

        // DeliveryTimeEstimation configuration
        modelBuilder.Entity<DeliveryTimeEstimation>(entity =>
        {
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Warehouse)
                  .WithMany()
                  .HasForeignKey(e => e.WarehouseId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.HasIndex(e => new { e.ProductId, e.CategoryId, e.WarehouseId, e.City });
            entity.HasIndex(e => e.IsActive);
        });

        // Organization configuration
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TaxNumber).HasMaxLength(50);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasIndex(e => e.TaxNumber);
            entity.HasIndex(e => e.Status);
        });

        // Team configuration
        modelBuilder.Entity<Team>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Organization)
                  .WithMany(e => e.Teams)
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.TeamLead)
                  .WithMany()
                  .HasForeignKey(e => e.TeamLeadId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.OrganizationId, e.IsActive });
        });

        // TeamMember configuration
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.HasOne(e => e.Team)
                  .WithMany(e => e.Members)
                  .HasForeignKey(e => e.TeamId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                  .WithMany(e => e.TeamMemberships)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.TeamId, e.UserId }).IsUnique();
        });

        // User Organization relationship
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(e => e.Organization)
                  .WithMany(e => e.Users)
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ✅ SECURITY: RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.IsRevoked, e.ExpiresAt });
            entity.Property(e => e.Token).IsRequired().HasMaxLength(256);
            entity.Property(e => e.CreatedByIp).HasMaxLength(50);
            entity.Property(e => e.RevokedByIp).HasMaxLength(50);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(256);
            entity.HasOne(e => e.User)
                  .WithMany(e => e.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // B2BUser configuration
        modelBuilder.Entity<B2BUser>(entity =>
        {
            entity.Property(e => e.EmployeeId).HasMaxLength(50);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
            entity.Property(e => e.UsedCredit).HasPrecision(18, 2);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Organization)
                  .WithMany()
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ApprovedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.UserId, e.OrganizationId }).IsUnique();
        });

        // WholesalePrice configuration
        modelBuilder.Entity<WholesalePrice>(entity =>
        {
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Organization)
                  .WithMany()
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.ProductId, e.OrganizationId, e.MinQuantity });
        });

        // CreditTerm configuration
        modelBuilder.Entity<CreditTerm>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
            entity.Property(e => e.UsedCredit).HasPrecision(18, 2);
            entity.HasOne(e => e.Organization)
                  .WithMany()
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.OrganizationId, e.IsActive });
        });

        // PurchaseOrder configuration
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.Property(e => e.PONumber).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.PONumber).IsUnique();
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.Tax).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.HasOne(e => e.Organization)
                  .WithMany()
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.B2BUser)
                  .WithMany(e => e.PurchaseOrders)
                  .HasForeignKey(e => e.B2BUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ApprovedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.CreditTerm)
                  .WithMany()
                  .HasForeignKey(e => e.CreditTermId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.OrganizationId, e.Status });
        });

        // PurchaseOrderItem configuration
        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(18, 2);
            entity.HasOne(e => e.PurchaseOrder)
                  .WithMany(e => e.Items)
                  .HasForeignKey(e => e.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // VolumeDiscount configuration
        modelBuilder.Entity<VolumeDiscount>(entity =>
        {
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            entity.Property(e => e.FixedDiscountAmount).HasPrecision(18, 2);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Category)
                  .WithMany()
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Organization)
                  .WithMany()
                  .HasForeignKey(e => e.OrganizationId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.ProductId, e.CategoryId, e.OrganizationId, e.MinQuantity });
        });

        // SubscriptionPlan configuration
        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PlanType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.SetupFee).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.BillingCycle).HasMaxLength(50);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.PlanType);
        });

        // UserSubscription configuration
        modelBuilder.Entity<UserSubscription>(entity =>
        {
            // ✅ ARCHITECTURE: Status artık enum (string değil)
            entity.Property(e => e.CurrentPrice).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethodId).HasMaxLength(100);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.SubscriptionPlan)
                  .WithMany(e => e.UserSubscriptions)
                  .HasForeignKey(e => e.SubscriptionPlanId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // ✅ PERFORMANCE: Database Indexes (BOLUM 6.5)
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.UserId, e.Status });
            entity.HasIndex(e => e.EndDate);
            entity.HasIndex(e => e.NextBillingDate);
            
            // ✅ SECURITY: Check Constraints (BOLUM 7.3)
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_UserSubscription_CurrentPrice_NonNegative", "\"CurrentPrice\" >= 0");
                t.HasCheckConstraint("CK_UserSubscription_RenewalCount_NonNegative", "\"RenewalCount\" >= 0");
            });
        });

        // SubscriptionPayment configuration
        modelBuilder.Entity<SubscriptionPayment>(entity =>
        {
            // ✅ ARCHITECTURE: PaymentStatus artık enum (string değil)
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.TransactionId).HasMaxLength(200);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.HasOne(e => e.UserSubscription)
                  .WithMany(e => e.Payments)
                  .HasForeignKey(e => e.UserSubscriptionId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // ✅ PERFORMANCE: Database Indexes (BOLUM 6.5)
            entity.HasIndex(e => e.UserSubscriptionId);
            entity.HasIndex(e => e.PaymentStatus);
            entity.HasIndex(e => new { e.UserSubscriptionId, e.PaymentStatus });
            entity.HasIndex(e => e.TransactionId);
            entity.HasIndex(e => e.CreatedAt);
            
            // ✅ SECURITY: Check Constraints (BOLUM 7.3)
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_SubscriptionPayment_Amount_Positive", "\"Amount\" >= 0");
                t.HasCheckConstraint("CK_SubscriptionPayment_RetryCount_NonNegative", "\"RetryCount\" >= 0");
            });
        });

        // SubscriptionUsage configuration
        modelBuilder.Entity<SubscriptionUsage>(entity =>
        {
            entity.Property(e => e.Feature).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.UserSubscription)
                  .WithMany()
                  .HasForeignKey(e => e.UserSubscriptionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserSubscriptionId, e.Feature, e.PeriodStart });
        });

        // BlogCategory configuration
        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasOne(e => e.ParentCategory)
                  .WithMany(e => e.SubCategories)
                  .HasForeignKey(e => e.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.IsActive);
        });

        // BlogPost configuration
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.Category)
                  .WithMany(e => e.Posts)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Author)
                  .WithMany()
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.Status, e.PublishedAt });
            entity.HasIndex(e => e.IsFeatured);
        });

        // BlogComment configuration
        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.Property(e => e.AuthorName).HasMaxLength(200);
            entity.Property(e => e.AuthorEmail).HasMaxLength(255);
            entity.HasOne(e => e.BlogPost)
                  .WithMany(e => e.Comments)
                  .HasForeignKey(e => e.BlogPostId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ParentComment)
                  .WithMany(e => e.Replies)
                  .HasForeignKey(e => e.ParentCommentId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.BlogPostId, e.IsApproved });
        });

        // BlogPostView configuration
        modelBuilder.Entity<BlogPostView>(entity =>
        {
            entity.HasOne(e => e.BlogPost)
                  .WithMany(e => e.Views)
                  .HasForeignKey(e => e.BlogPostId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.BlogPostId, e.CreatedAt });
        });

        // SEOSettings configuration
        modelBuilder.Entity<SEOSettings>(entity =>
        {
            entity.Property(e => e.PageType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MetaTitle).HasMaxLength(70); // Recommended length
            entity.Property(e => e.MetaDescription).HasMaxLength(160); // Recommended length
            entity.Property(e => e.CanonicalUrl).HasMaxLength(500);
            entity.Property(e => e.ChangeFrequency).HasMaxLength(20);
            entity.Property(e => e.Priority).HasPrecision(2, 1);
            entity.HasIndex(e => new { e.PageType, e.EntityId }).IsUnique();
        });

        // SitemapEntry configuration
        modelBuilder.Entity<SitemapEntry>(entity =>
        {
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
            entity.Property(e => e.PageType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ChangeFrequency).HasMaxLength(20);
            entity.Property(e => e.Priority).HasPrecision(2, 1);
            entity.HasIndex(e => new { e.PageType, e.EntityId });
            entity.HasIndex(e => e.IsActive);
        });

        // CMSPage configuration
        modelBuilder.Entity<CMSPage>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.PageType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.Author)
                  .WithMany()
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ParentPage)
                  .WithMany(e => e.ChildPages)
                  .HasForeignKey(e => e.ParentPageId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.Status, e.IsHomePage });
            entity.HasIndex(e => e.ShowInMenu);
        });

        // LandingPage configuration
        modelBuilder.Entity<LandingPage>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.ConversionRate).HasPrecision(5, 2);
            entity.HasOne(e => e.Author)
                  .WithMany()
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.VariantOf)
                  .WithMany(e => e.Variants)
                  .HasForeignKey(e => e.VariantOfId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.Status, e.IsActive });
            entity.HasIndex(e => e.EnableABTesting);
        });

        // LiveChatSession configuration
        modelBuilder.Entity<LiveChatSession>(entity =>
        {
            entity.Property(e => e.SessionId).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.SessionId).IsUnique();
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.GuestName).HasMaxLength(200);
            entity.Property(e => e.GuestEmail).HasMaxLength(255);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Agent)
                  .WithMany()
                  .HasForeignKey(e => e.AgentId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.Status, e.AgentId });
            entity.HasIndex(e => e.StartedAt);
        });

        // LiveChatMessage configuration
        modelBuilder.Entity<LiveChatMessage>(entity =>
        {
            entity.Property(e => e.SenderType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MessageType).HasMaxLength(50);
            entity.HasOne(e => e.Session)
                  .WithMany(e => e.Messages)
                  .HasForeignKey(e => e.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Sender)
                  .WithMany()
                  .HasForeignKey(e => e.SenderId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.SessionId, e.CreatedAt });
        });

        // FraudDetectionRule configuration
        modelBuilder.Entity<FraudDetectionRule>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RuleType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Action).HasMaxLength(50);
            entity.HasIndex(e => new { e.RuleType, e.IsActive });
            entity.HasIndex(e => e.Priority);
        });

        // FraudAlert configuration
        modelBuilder.Entity<FraudAlert>(entity =>
        {
            entity.Property(e => e.AlertType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Order)
                  .WithMany()
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Payment)
                  .WithMany()
                  .HasForeignKey(e => e.PaymentId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ReviewedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.Status, e.AlertType });
            entity.HasIndex(e => e.RiskScore);
        });

        // OAuthProvider configuration
        modelBuilder.Entity<OAuthProvider>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProviderKey).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.ProviderKey).IsUnique();
            entity.Property(e => e.ClientId).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ClientSecret).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.IsActive);
        });

        // OAuthAccount configuration
        modelBuilder.Entity<OAuthAccount>(entity =>
        {
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProviderUserId).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => new { e.Provider, e.ProviderUserId }).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.UserId);
        });

        // PushNotificationDevice configuration
        modelBuilder.Entity<PushNotificationDevice>(entity =>
        {
            entity.Property(e => e.DeviceToken).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Platform).IsRequired().HasMaxLength(50);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.DeviceToken });
            entity.HasIndex(e => e.IsActive);
        });

        // PushNotification configuration
        modelBuilder.Entity<PushNotification>(entity =>
        {
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.NotificationType).HasMaxLength(50);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.Device)
                  .WithMany()
                  .HasForeignKey(e => e.DeviceId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.Status, e.NotificationType });
        });

        // InternationalShipping configuration
        modelBuilder.Entity<InternationalShipping>(entity =>
        {
            entity.Property(e => e.OriginCountry).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DestinationCountry).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ShippingMethod).HasMaxLength(50);
            entity.Property(e => e.ShippingCost).HasPrecision(18, 2);
            entity.Property(e => e.CustomsDuty).HasPrecision(18, 2);
            entity.Property(e => e.ImportTax).HasPrecision(18, 2);
            entity.Property(e => e.HandlingFee).HasPrecision(18, 2);
            entity.Property(e => e.TotalCost).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.Order)
                  .WithMany()
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.OrderId, e.Status });
        });

        // TaxRule configuration
        modelBuilder.Entity<TaxRule>(entity =>
        {
            entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.TaxType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.HasIndex(e => new { e.Country, e.State, e.City, e.IsActive });
            entity.HasIndex(e => e.TaxType);
        });

        // CustomsDeclaration configuration
        modelBuilder.Entity<CustomsDeclaration>(entity =>
        {
            entity.Property(e => e.DeclarationNumber).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.DeclarationNumber).IsUnique();
            entity.Property(e => e.OriginCountry).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DestinationCountry).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TotalValue).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.CustomsDuty).HasPrecision(18, 2);
            entity.Property(e => e.ImportTax).HasPrecision(18, 2);
            entity.HasOne(e => e.Order)
                  .WithMany()
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.OrderId, e.Status });
        });

        // OrderVerification configuration
        modelBuilder.Entity<OrderVerification>(entity =>
        {
            entity.Property(e => e.VerificationType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.VerificationMethod).HasMaxLength(100);
            entity.HasOne(e => e.Order)
                  .WithMany()
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.VerifiedBy)
                  .WithMany()
                  .HasForeignKey(e => e.VerifiedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.OrderId, e.Status });
            entity.HasIndex(e => e.RequiresManualReview);
        });

        // PaymentFraudPrevention configuration
        modelBuilder.Entity<PaymentFraudPrevention>(entity =>
        {
            entity.Property(e => e.CheckType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.DeviceFingerprint).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.HasOne(e => e.Payment)
                  .WithMany()
                  .HasForeignKey(e => e.PaymentId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.PaymentId, e.Status });
            entity.HasIndex(e => e.IsBlocked);
        });

        // AccountSecurityEvent configuration
        modelBuilder.Entity<AccountSecurityEvent>(entity =>
        {
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Severity).HasMaxLength(20);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.DeviceFingerprint).HasMaxLength(500);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ActionTakenBy)
                  .WithMany()
                  .HasForeignKey(e => e.ActionTakenByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.UserId, e.EventType, e.CreatedAt });
            entity.HasIndex(e => e.IsSuspicious);
        });

        // SecurityAlert configuration
        modelBuilder.Entity<SecurityAlert>(entity =>
        {
            entity.Property(e => e.AlertType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Severity).HasMaxLength(20);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.AcknowledgedBy)
                  .WithMany()
                  .HasForeignKey(e => e.AcknowledgedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.ResolvedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ResolvedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.Status, e.Severity, e.AlertType });
            entity.HasIndex(e => e.UserId);
        });

        // LiveStream configuration
        modelBuilder.Entity<LiveStream>(entity =>
        {
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.Seller)
                  .WithMany()
                  .HasForeignKey(e => e.SellerId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.SellerId, e.Status, e.ScheduledStartTime });
        });

        // LiveStreamProduct configuration
        modelBuilder.Entity<LiveStreamProduct>(entity =>
        {
            entity.HasOne(e => e.LiveStream)
                  .WithMany(s => s.Products)
                  .HasForeignKey(e => e.LiveStreamId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.LiveStreamId, e.ProductId });
        });

        // LiveStreamViewer configuration
        modelBuilder.Entity<LiveStreamViewer>(entity =>
        {
            entity.HasOne(e => e.LiveStream)
                  .WithMany(s => s.Viewers)
                  .HasForeignKey(e => e.LiveStreamId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.LiveStreamId, e.UserId, e.IsActive });
        });

        // LiveStreamOrder configuration
        modelBuilder.Entity<LiveStreamOrder>(entity =>
        {
            entity.HasOne(e => e.LiveStream)
                  .WithMany(s => s.Orders)
                  .HasForeignKey(e => e.LiveStreamId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Order)
                  .WithMany()
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.LiveStreamId, e.OrderId });
        });

        // PageBuilder configuration
        modelBuilder.Entity<PageBuilder>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.Author)
                  .WithMany()
                  .HasForeignKey(e => e.AuthorId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => new { e.Slug, e.Status });
        });

        // DataWarehouse configuration
        modelBuilder.Entity<DataWarehouse>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasIndex(e => new { e.Name, e.Status });
        });

        // ETLProcess configuration
        modelBuilder.Entity<ETLProcess>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasIndex(e => new { e.Status, e.NextRunAt });
        });

        // DataPipeline configuration
        modelBuilder.Entity<DataPipeline>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasIndex(e => new { e.Status, e.NextRunAt });
        });

        // DataQualityRule configuration
        modelBuilder.Entity<DataQualityRule>(entity =>
        {
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RuleType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasIndex(e => new { e.TargetEntity, e.Status });
        });

        // DataQualityCheck configuration
        modelBuilder.Entity<DataQualityCheck>(entity =>
        {
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.HasOne(e => e.Rule)
                  .WithMany()
                  .HasForeignKey(e => e.RuleId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.RuleId, e.CheckedAt });
        });
    }

    /// <summary>
    /// ✅ PERFORMANCE OPTIMIZATION: Global Query Filter for Soft Delete
    /// Automatically filters out soft-deleted entities across all queries
    /// Eliminates need for manual "!IsDeleted" checks in 500+ locations
    /// </summary>
    private void ConfigureGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Get all entity types that inherit from BaseEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Check if entity inherits from BaseEntity (has IsDeleted property)
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Create expression: e => !e.IsDeleted
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(
                    Expression.Equal(property, Expression.Constant(false)),
                    parameter
                );

                // Apply global query filter
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        // Note: User entity doesn't inherit from BaseEntity but has IsDeleted
        // Apply filter separately for User
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
    }
}

