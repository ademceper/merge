using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;
using Merge.Application.Common.Behaviors;
using Merge.Application.Interfaces.Analytics;
using Merge.Application.Interfaces.B2B;
using Merge.Application.Interfaces.Cart;

using Merge.Application.Interfaces.Content;

using Merge.Application.Interfaces.Logistics;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Interfaces.ML;
using Merge.Application.Interfaces.Notification;

using Merge.Application.Interfaces.Organization;

using Merge.Application.Interfaces.Product;
using Merge.Application.Interfaces.Review;
using Merge.Application.Interfaces.Search;
using Merge.Application.Interfaces.Security;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Interfaces.Governance;
using Merge.Application.Services.Analytics;
using Merge.Application.Services.B2B;
using Merge.Application.Services.Cart;
using Merge.Application.Services.Order;
using Merge.Application.Services.Payment;
using Merge.Application.Interfaces.Order;
using Merge.Application.Interfaces.Payment;

using Merge.Application.Services.Content;
using Merge.Application.Services.Governance;

using Merge.Application.Services.Logistics;
using Merge.Application.Services.Marketing;
using Merge.Application.Services.ML;
using Merge.Application.Services.Notification;

using Merge.Application.Services.Organization;

using Merge.Application.Services.Product;
using Merge.Application.Services.Review;
using Merge.Application.Services.Search;
using Merge.Application.Services.Seller;

namespace Merge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        // ✅ BOLUM 1.1: Application Services (Managed transition to CQRS)
        // #pragma warning disable CS0618





        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
        services.AddScoped<IReturnRequestService, ReturnRequestService>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IProductBundleService, ProductBundleService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<ISellerDashboardService, SellerDashboardService>();

        services.AddScoped<ISavedCartService, SavedCartService>();
        services.AddScoped<IBannerService, BannerService>();


        services.AddScoped<IBulkProductService, BulkProductService>();

        services.AddScoped<ISellerOnboardingService, SellerOnboardingService>();
        services.AddScoped<IAbandonedCartService, AbandonedCartService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ISharedWishlistService, SharedWishlistService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<AdminService>(); // Concrete or interface? Program.cs used concrete for some
        services.AddScoped<IProductComparisonService, ProductComparisonService>();
        services.AddScoped<ISizeGuideService, SizeGuideService>();
        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
        services.AddScoped<ISellerFinanceService, SellerFinanceService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IProductTemplateService, ProductTemplateService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IBlogService, BlogService>();
        services.AddScoped<ISEOService, SEOService>();
        services.AddScoped<IProductQuestionService, ProductQuestionService>();
        services.AddScoped<ISellerCommissionService, SellerCommissionService>();
        services.AddScoped<ICMSService, CMSService>();
        services.AddScoped<ILandingPageService, LandingPageService>();
        services.AddScoped<IFraudDetectionService, FraudDetectionService>();
        services.AddScoped<IPriceOptimizationService, PriceOptimizationService>();
        services.AddScoped<IDemandForecastingService, DemandForecastingService>();
        services.AddScoped<IElasticsearchService, ElasticsearchService>();
        services.AddScoped<IPersonalizationService, PersonalizationService>();
        services.AddScoped<IPageBuilderService, PageBuilderService>();
        // #pragma warning restore CS0618

        return services;
    }
}
