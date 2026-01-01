using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Merge.Application.Interfaces.Analytics;
using Merge.Application.Interfaces.B2B;
using Merge.Application.Interfaces.Cart;
using Merge.Application.Interfaces.Catalog;
using Merge.Application.Interfaces.Content;
using Merge.Application.Interfaces.EmailProviders;
using Merge.Application.Interfaces.Governance;
using Merge.Application.Interfaces.Identity;
using Merge.Application.Interfaces.International;
using Merge.Application.Interfaces.LiveCommerce;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Interfaces.ML;
using Merge.Application.Interfaces.Notification;
using Merge.Application.Interfaces.Order;
using Merge.Application.Interfaces.Organization;
using Merge.Application.Interfaces.Payment;
using Merge.Application.Interfaces.PaymentGateways;
using Merge.Application.Interfaces.Product;
using Merge.Application.Interfaces.Review;
using Merge.Application.Interfaces.Search;
using Merge.Application.Interfaces.Security;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Interfaces.ShippingProviders;
using Merge.Application.Interfaces.SmsProviders;
using Merge.Application.Interfaces.Subscription;
using Merge.Application.Interfaces.Support;
using Merge.Application.Interfaces.User;
using Merge.Application.Services;
using Merge.Application.Services.Content;
using Merge.Application.Services.International;
using Merge.Application.Services.B2B;
using Merge.Application.Services.Subscription;
using Merge.Application.Services.LiveCommerce;
using Merge.Application.Services.Governance;
using Merge.Application.Services.Analytics;
using Merge.Application.Services.ML;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Domain.Entities;
using Merge.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Merge E-Commerce API",
        Version = "v1",
        Description = "e-ticaret backend API"
    });

    // JWT Authentication için Swagger yapılandırması
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database configuration
// ✅ SECURITY: Connection string önce environment variable'dan al
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Database connection string bulunamadı. DATABASE_URL environment variable veya appsettings ConnectionStrings:DefaultConnection tanımlayın.");

// ✅ SECURITY: Production'da varsayılan password kullanımını engelle
if (!builder.Environment.IsDevelopment() && connectionString.Contains("Password=postgres"))
{
    throw new InvalidOperationException("CRITICAL SECURITY ERROR: Production'da varsayılan database password kullanılamaz! DATABASE_URL environment variable tanımlayın.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Memory Cache configuration for performance optimization
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Maximum number of cache entries
});

// Identity configuration
builder.Services.AddIdentity<User, Role>(options =>
{
    // ✅ SECURITY: Güçlü password policy (BOLUM 5.3)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.Password.RequiredUniqueChars = 4;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Email confirmation için false (şimdilik)
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Repository pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application services
builder.Services.AddScoped<IAuthService, Merge.Application.Services.Identity.AuthService>();
builder.Services.AddScoped<IProductService, Merge.Application.Services.Product.ProductService>();
builder.Services.AddScoped<ICategoryService, Merge.Application.Services.Catalog.CategoryService>();
builder.Services.AddScoped<ICartService, Merge.Application.Services.Cart.CartService>();
builder.Services.AddScoped<IOrderService, Merge.Application.Services.Order.OrderService>();
builder.Services.AddScoped<IOrderSplitService, Merge.Application.Services.Order.OrderSplitService>();
builder.Services.AddScoped<IWishlistService, Merge.Application.Services.Cart.WishlistService>();
builder.Services.AddScoped<ICouponService, Merge.Application.Services.Marketing.CouponService>();
builder.Services.AddScoped<INotificationService, Merge.Application.Services.Notification.NotificationService>();
builder.Services.AddScoped<INotificationTemplateService, Merge.Application.Services.Notification.NotificationTemplateService>();
builder.Services.AddScoped<ITrustBadgeService, Merge.Application.Services.Review.TrustBadgeService>();
builder.Services.AddScoped<IReviewService, Merge.Application.Services.Review.ReviewService>();
builder.Services.AddScoped<IReturnRequestService, Merge.Application.Services.Order.ReturnRequestService>();
builder.Services.AddScoped<IPaymentService, Merge.Application.Services.Payment.PaymentService>();
builder.Services.AddScoped<IShippingService, Merge.Application.Services.Logistics.ShippingService>();
builder.Services.AddScoped<IAddressService, Merge.Application.Services.User.AddressService>();
// FileUploadService - implement edilmediği için şimdilik yorum satırı
// builder.Services.AddScoped<IFileUploadService, Merge.Application.Services.Common.FileUploadService>();
builder.Services.AddScoped<IProductSearchService, Merge.Application.Services.Search.ProductSearchService>();
builder.Services.AddScoped<Merge.Application.Services.Notification.IEmailService, Merge.Application.Services.Notification.EmailService>();
builder.Services.AddScoped<Merge.Application.Services.Notification.ISmsService, Merge.Application.Services.Notification.SmsService>();
builder.Services.AddScoped<IFlashSaleService, Merge.Application.Services.Marketing.FlashSaleService>();
builder.Services.AddScoped<IProductBundleService, Merge.Application.Services.Product.ProductBundleService>();
builder.Services.AddScoped<IRecentlyViewedService, Merge.Application.Services.Cart.RecentlyViewedService>();
builder.Services.AddScoped<IInvoiceService, Merge.Application.Services.Payment.InvoiceService>();
builder.Services.AddScoped<ISellerDashboardService, Merge.Application.Services.Seller.SellerDashboardService>();
builder.Services.AddScoped<IEmailVerificationService, Merge.Application.Services.Identity.EmailVerificationService>();
builder.Services.AddScoped<ISavedCartService, Merge.Application.Services.Cart.SavedCartService>();
builder.Services.AddScoped<IFaqService, Merge.Application.Services.Support.FaqService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IGiftCardService, Merge.Application.Services.Marketing.GiftCardService>();
builder.Services.AddScoped<IOrderFilterService, Merge.Application.Services.Order.OrderFilterService>();
builder.Services.AddScoped<IWarehouseService, Merge.Application.Services.Logistics.WarehouseService>();
builder.Services.AddScoped<IInventoryService, Merge.Application.Services.Catalog.InventoryService>();
builder.Services.AddScoped<IStockMovementService, Merge.Application.Services.Logistics.StockMovementService>();
builder.Services.AddScoped<IBulkProductService, Merge.Application.Services.Product.BulkProductService>();
builder.Services.AddScoped<ITwoFactorAuthService, Merge.Application.Services.Identity.TwoFactorAuthService>();
builder.Services.AddScoped<ISellerOnboardingService, Merge.Application.Services.Seller.SellerOnboardingService>();
builder.Services.AddScoped<IProductRecommendationService, Merge.Application.Services.Search.ProductRecommendationService>();
builder.Services.AddScoped<ISearchSuggestionService, Merge.Application.Services.Search.SearchSuggestionService>();
builder.Services.AddScoped<IAbandonedCartService, Merge.Application.Services.Cart.AbandonedCartService>();
builder.Services.AddScoped<IUserActivityService, Merge.Application.Services.User.UserActivityService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<ILoyaltyService, Merge.Application.Services.Marketing.LoyaltyService>();
builder.Services.AddScoped<IReferralService, Merge.Application.Services.Marketing.ReferralService>();
builder.Services.AddScoped<IReviewMediaService, Merge.Application.Services.Marketing.ReviewMediaService>();
builder.Services.AddScoped<ISharedWishlistService, Merge.Application.Services.Marketing.SharedWishlistService>();
builder.Services.AddScoped<IEmailCampaignService, Merge.Application.Services.Marketing.EmailCampaignService>();
builder.Services.AddScoped<IAnalyticsService, Merge.Application.Services.Analytics.AnalyticsService>();
builder.Services.AddScoped<Merge.Application.Interfaces.Analytics.IAdminService, Merge.Application.Services.Analytics.AdminService>();
builder.Services.AddScoped<IProductComparisonService, Merge.Application.Services.Product.ProductComparisonService>();
builder.Services.AddScoped<IPreOrderService, Merge.Application.Services.Cart.PreOrderService>();
builder.Services.AddScoped<ISizeGuideService, Merge.Application.Services.Product.SizeGuideService>();
builder.Services.AddScoped<IReviewHelpfulnessService, Merge.Application.Services.Review.ReviewHelpfulnessService>();
builder.Services.AddScoped<ISupportTicketService, Merge.Application.Services.Support.SupportTicketService>();
builder.Services.AddScoped<ITrustBadgeService, Merge.Application.Services.Review.TrustBadgeService>();
builder.Services.AddScoped<IKnowledgeBaseService, Merge.Application.Services.Support.KnowledgeBaseService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<INotificationPreferenceService, Merge.Application.Services.Notification.NotificationPreferenceService>();
builder.Services.AddScoped<ICustomerCommunicationService, Merge.Application.Services.Support.CustomerCommunicationService>();
builder.Services.AddScoped<ISellerFinanceService, Merge.Application.Services.Seller.SellerFinanceService>();
builder.Services.AddScoped<IStoreService, Merge.Application.Services.Seller.StoreService>();
builder.Services.AddScoped<IProductTemplateService, Merge.Application.Services.Product.ProductTemplateService>();
builder.Services.AddScoped<IShippingAddressService, Merge.Application.Services.Logistics.ShippingAddressService>();
builder.Services.AddScoped<IPaymentMethodService, Merge.Application.Services.Payment.PaymentMethodService>();
builder.Services.AddScoped<IPickPackService, Merge.Application.Services.Logistics.PickPackService>();
builder.Services.AddScoped<IDeliveryTimeEstimationService, Merge.Application.Services.Logistics.DeliveryTimeEstimationService>();
builder.Services.AddScoped<IOrganizationService, Merge.Application.Services.Organization.OrganizationService>();
builder.Services.AddScoped<IB2BService, B2BService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<ISEOService, Merge.Application.Services.Content.SEOService>();
builder.Services.AddScoped<IProductQuestionService, Merge.Application.Services.Product.ProductQuestionService>();
builder.Services.AddScoped<ISellerCommissionService, Merge.Application.Services.Seller.SellerCommissionService>();
builder.Services.AddScoped<ICMSService, Merge.Application.Services.Content.CMSService>();
builder.Services.AddScoped<ILandingPageService, Merge.Application.Services.Content.LandingPageService>();
builder.Services.AddScoped<Merge.Application.Interfaces.Support.ILiveChatService, Merge.Application.Services.Support.LiveChatService>();
builder.Services.AddScoped<IFraudDetectionService, Merge.Application.Services.ML.FraudDetectionService>();
builder.Services.AddScoped<IOrderVerificationService, Merge.Application.Services.Security.OrderVerificationService>();
builder.Services.AddScoped<IPaymentFraudPreventionService, Merge.Application.Services.Security.PaymentFraudPreventionService>();
builder.Services.AddScoped<IAccountSecurityMonitoringService, Merge.Application.Services.Security.AccountSecurityMonitoringService>();

// Payment Gateways
builder.Services.AddScoped<Merge.Application.Services.PaymentGateways.IyzicoGateway>();
builder.Services.AddScoped<Merge.Application.Services.PaymentGateways.PayTRGateway>();
builder.Services.AddScoped<Merge.Application.Services.PaymentGateways.StripeGateway>();
builder.Services.AddScoped<Merge.Application.Services.PaymentGateways.PaymentGatewayFactory>();

// Shipping Providers
builder.Services.AddScoped<Merge.Application.Services.ShippingProviders.YurticiProvider>();
builder.Services.AddScoped<Merge.Application.Services.ShippingProviders.ArasProvider>();
builder.Services.AddScoped<Merge.Application.Services.ShippingProviders.MNGProvider>();
builder.Services.AddScoped<Merge.Application.Services.ShippingProviders.ShippingProviderFactory>();

// Email Providers
builder.Services.AddScoped<Merge.Application.Services.EmailProviders.SendGridProvider>();

// SMS Providers
builder.Services.AddScoped<Merge.Application.Services.SmsProviders.TwilioProvider>();
builder.Services.AddScoped<Merge.Application.Services.SmsProviders.NetgsmProvider>();

// Personalization
builder.Services.AddScoped<Merge.Application.Services.Search.IPersonalizationService, Merge.Application.Services.Search.PersonalizationService>();

// Live Commerce
builder.Services.AddScoped<ILiveCommerceService, Merge.Application.Services.LiveCommerce.LiveCommerceService>();

// Page Builder
builder.Services.AddScoped<IPageBuilderService, Merge.Application.Services.Content.PageBuilderService>();

// Price Optimization
builder.Services.AddScoped<IPriceOptimizationService, Merge.Application.Services.ML.PriceOptimizationService>();

// Demand Forecasting
builder.Services.AddScoped<IDemandForecastingService, Merge.Application.Services.ML.DemandForecastingService>();

// Data Platform - implement edilmediği için şimdilik yorum satırı
// builder.Services.AddScoped<IDataPlatformService, Merge.Application.Services.DataPlatform.DataPlatformService>();

// Elasticsearch (with SQL fallback)
builder.Services.AddScoped<IElasticsearchService, Merge.Application.Services.Search.ElasticsearchService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Merge.Application.Mappings.MappingProfile));

// JWT Authentication
// ✅ SECURITY: JWT Secret önce environment variable'dan al, yoksa appsettings'ten
var jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key bulunamadı. JWT_SECRET_KEY environment variable veya appsettings Jwt:Key tanımlayın.");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
    ?? builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer bulunamadı");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
    ?? builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience bulunamadı");

// ✅ SECURITY: Production'da hardcoded key kullanımını engelle
if (!builder.Environment.IsDevelopment() && jwtKey == "YourSuperSecretKeyThatIsAtLeast32CharactersLong!")
{
    throw new InvalidOperationException("CRITICAL SECURITY ERROR: Production'da varsayılan JWT key kullanılamaz! JWT_SECRET_KEY environment variable tanımlayın.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ✅ SECURITY: Rate Limiting (BOLUM 3.3)
// Note: Rate limiting is handled by custom RateLimitingMiddleware
// Configuration is done via RateLimitAttribute on controllers

// Session for rate limiting (optional, if not using distributed cache)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// CORS
// ✅ SECURITY: Production için güvenli CORS yapılandırması
builder.Services.AddCors(options =>
{
    // Development için gevşek policy
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // ✅ SECURITY: Production için güvenli CORS policy
    options.AddPolicy("Production", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "https://mergecommerce.com", "https://www.mergecommerce.com" };

        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "X-CSRF-TOKEN")
              .AllowCredentials()
              .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Merge E-Commerce API v1");
    });
}

// Global Exception Handler - En üstte olmalı
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

// ✅ SECURITY: Security Headers (BOLUM 5.2)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    }
    await next();
});

// ✅ SECURITY: Development'ta AllowAll, Production'da güvenli CORS policy kullan
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
}
else
{
    app.UseCors("Production");
}

app.UseStaticFiles(); // wwwroot için

// Security middlewares
app.UseRateLimiting();
app.UseIpWhitelist();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ✅ CRITICAL FIX: Database migration strategy
// BEFORE: EnsureCreated() bypasses migrations (causes data loss, no rollback)
// AFTER: Migrate() runs pending migrations properly
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            // ✅ Use Migrate() instead of EnsureCreated()
            // This runs pending migrations and maintains migration history
            dbContext.Database.Migrate();

            logger.LogInformation("Database migration completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed. Error: {ErrorMessage}", ex.Message);
            // Re-throw in development to catch migration issues early
            throw;
        }
    }
}

// Note: For production, run migrations via CI/CD pipeline:
// dotnet ef database update --project Merge.Infrastructure --startup-project Merge.API

app.Run();
