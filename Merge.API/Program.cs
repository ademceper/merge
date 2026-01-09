using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
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
using Merge.Domain.Interfaces;
using Merge.Domain.Entities;
using Merge.API.Middleware;
using Merge.Application.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ✅ BOLUM 5.0: OpenTelemetry tracing + metrics (ZORUNLU)
// TODO: OpenTelemetry paket versiyonları uyumlu değil - Production'da güncellenmeli
// Şimdilik yorum satırına alındı - Paket versiyonları düzeltildikten sonra aktif edilecek
// Production'da tam OpenTelemetry setup için:
// - OpenTelemetry.Instrumentation.AspNetCore versiyonları güncellenmeli
// - OpenTelemetry.Exporter.Prometheus.AspNetCore versiyonları güncellenmeli
// - Jaeger exporter eklenebilir
/*
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());
*/

// ✅ CONFIGURATION: Business settings (BEST_PRACTICES_ANALIZI.md - BOLUM 2.1.4)
builder.Services.Configure<Merge.Application.Configuration.OrderSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.OrderSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.PaymentSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.PaymentSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.LoyaltySettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.LoyaltySettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.ReferralSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.ReferralSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.B2BSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.B2BSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.AnalyticsSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.AnalyticsSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.CartSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.CartSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.ContentSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.ContentSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.PaginationSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.PaginationSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.SellerSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.SellerSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.EmailSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.EmailSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.SecuritySettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.SecuritySettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.JwtSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.JwtSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.TwoFactorAuthSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.TwoFactorAuthSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.SupportSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.SupportSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.MLSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.MLSettings.SectionName));
builder.Services.Configure<Merge.Application.Configuration.ServiceSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.ServiceSettings.SectionName));

// Add services to the container
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
builder.Services.AddControllers();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    // ✅ GroupNameFormat "'v'V" - sadece major version (v1, v2, vb.)
    // Bu, Swagger endpoint'lerinin /swagger/v1/swagger.json formatında olmasını sağlar
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
builder.Services.AddSwaggerGen(options =>
{
    // ✅ API Versioning ile uyumlu Swagger yapılandırması
    // IApiVersionDescriptionProvider kullanarak her versiyon için dinamik SwaggerDoc oluştur
    // Bu, "No operations defined in spec!" hatasını çözer
    // GroupNameFormat "'v'V" olduğu için "v1" formatı kullanılır
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Merge E-Commerce API",
        Version = "v1.0",
        Description = "e-ticaret backend API v1.0"
    });
    
    // API versioning için Swagger yapılandırması
    options.AddServer(new OpenApiServer { Url = "https://api.mergecommerce.com" });

    // ✅ XML documentation için (BOLUM 4.1)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // JWT Authentication için Swagger yapılandırması
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    
    // ✅ API Versioning için: ResolveConflictingActions kullanarak çakışmaları çöz
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    
    // ✅ API Versioning için: VersionedApiExplorer ile entegrasyon
    // Bu, Swagger'ın versioned API'leri bulmasını sağlar
    // GroupNameFormat "'v'V" olduğu için "v1" formatı kullanılır
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        // VersionedApiExplorer kullanıldığında, docName GroupNameFormat'e göre oluşturulur
        // GroupNameFormat "'v'V" olduğu için "v1" formatı kullanılır
        // apiDesc.GroupName ile docName'i karşılaştır
        return apiDesc.GroupName == docName;
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

// ✅ BOLUM 1.1: IDbContext registration (Clean Architecture)
// Service'ler ApplicationDbContext yerine IDbContext kullanmalı
builder.Services.AddScoped<Merge.Application.Interfaces.IDbContext, ApplicationDbContext>();

// ✅ BOLUM 5.0: Health Checks (ZORUNLU - Gerçek health check)
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString,
        name: "postgres",
        tags: new[] { "db", "postgres", "sql" })
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379",
        name: "redis",
        tags: new[] { "cache", "redis" });

// ✅ BOLUM 10.2: Redis distributed cache (ZORUNLU)
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "Merge:";
});

// ✅ BOLUM 10.2: Cache service registration
builder.Services.AddScoped<Merge.Application.Interfaces.ICacheService, Merge.Application.Services.CacheService>();

// ✅ BOLUM 10.0: Polly resilience (circuit breaker, retry) (ZORUNLU)
builder.Services.AddHttpClient("PaymentGateway")
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(2);
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.MinimumThroughput = 10;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
    });

builder.Services.AddHttpClient("ShippingProvider")
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(2);
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
    });

// Memory Cache configuration for performance optimization (fallback)
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024; // Maximum number of cache entries
});

// ✅ BOLUM 7.1: Response Compression (ZORUNLU)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "text/json", "application/xml", "text/xml" });
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
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
// ✅ BOLUM 1.5: Domain Events publish mekanizması (ZORUNLU)
builder.Services.AddScoped<Merge.Domain.Common.IDomainEventDispatcher, Merge.Infrastructure.Common.DomainEventDispatcher>();

// ✅ BOLUM 1.1: Clean Architecture - Application.Interfaces'den IRepository ve IUnitOfWork kullan
builder.Services.AddScoped(typeof(Merge.Application.Interfaces.IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<Merge.Application.Interfaces.IUnitOfWork, UnitOfWork>();

// Application services
builder.Services.AddScoped<IAuthService, Merge.Application.Services.Identity.AuthService>();
builder.Services.AddScoped<IProductService, Merge.Application.Services.Product.ProductService>();
builder.Services.AddScoped<ICategoryService, Merge.Application.Services.Catalog.CategoryService>();
builder.Services.AddScoped<IOrderService, Merge.Application.Services.Order.OrderService>();
builder.Services.AddScoped<IOrderSplitService, Merge.Application.Services.Order.OrderSplitService>();
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
builder.Services.AddScoped<ILoyaltyService, Merge.Application.Services.Marketing.LoyaltyService>();
builder.Services.AddScoped<IReferralService, Merge.Application.Services.Marketing.ReferralService>();
builder.Services.AddScoped<IReviewMediaService, Merge.Application.Services.Marketing.ReviewMediaService>();
builder.Services.AddScoped<ISharedWishlistService, Merge.Application.Services.Marketing.SharedWishlistService>();
builder.Services.AddScoped<IEmailCampaignService, Merge.Application.Services.Marketing.EmailCampaignService>();
// ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
builder.Services.Configure<Merge.Application.Configuration.AnalyticsSettings>(
    builder.Configuration.GetSection(Merge.Application.Configuration.AnalyticsSettings.SectionName));

builder.Services.AddScoped<IAnalyticsService, Merge.Application.Services.Analytics.AnalyticsService>();
builder.Services.AddScoped<Merge.Application.Interfaces.Analytics.IAdminService, Merge.Application.Services.Analytics.AdminService>();
builder.Services.AddScoped<IProductComparisonService, Merge.Application.Services.Product.ProductComparisonService>();
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
// ✅ ARCHITECTURE: B2BService kaldırıldı - Handler'lar direkt IDbContext kullanıyor (Clean Architecture)
// builder.Services.AddScoped<IB2BService, B2BService>(); // DEPRECATED - MediatR + CQRS pattern kullanılıyor
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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Merge.Application.Orders.Commands.CreateOrder.CreateOrderCommand).Assembly);
    // ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
    cfg.AddOpenBehavior(typeof(Merge.Application.Common.Behaviors.ValidationBehavior<,>));
});

// ✅ BOLUM 2.1: Pipeline Behaviors - FluentValidation validators (ZORUNLU)
builder.Services.AddValidatorsFromAssembly(typeof(Merge.Application.Orders.Commands.CreateOrder.CreateOrderCommandValidator).Assembly);

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
    
    // ✅ API Versioning ile uyumlu Swagger UI yapılandırması
    // IApiVersionDescriptionProvider kullanarak her versiyon için dinamik endpoint oluştur
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    
    app.UseSwaggerUI(c =>
    {
        // Her versiyon için dinamik olarak Swagger endpoint oluştur
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                $"Merge E-Commerce API {description.GroupName.ToUpperInvariant()}");
        }
        
        c.RoutePrefix = "swagger"; // Swagger UI: /swagger
    });
}

// Global Exception Handler - En üstte olmalı
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

// ✅ BOLUM 7.1: Response Compression (ZORUNLU)
app.UseResponseCompression();

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

// ✅ BOLUM 5.0: Health Checks (ZORUNLU - Gerçek health check)
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

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
            logger.LogWarning(ex, "Database migration failed. This is expected if PostgreSQL is not running. Error: {ErrorMessage}", ex.Message);
            logger.LogWarning("To start PostgreSQL, run: docker-compose up -d");
            logger.LogWarning("Or start PostgreSQL manually and ensure connection string is correct in appsettings.json");
            // ✅ Development'ta migration hatası uygulamayı crash etmesin
            // Production'da migration'lar CI/CD pipeline'da çalıştırılmalı (BOLUM 6.0)
        }
    }
}

// Note: For production, run migrations via CI/CD pipeline:
// dotnet ef database update --project Merge.Infrastructure --startup-project Merge.API

app.Run();
