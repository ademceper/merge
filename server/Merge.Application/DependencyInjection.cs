using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;
using Merge.Application.Common.Behaviors;

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
using Merge.Application.Services.Order;
using Merge.Application.Services.Payment;
using Merge.Application.Interfaces.Order;
using Merge.Application.Interfaces.Payment;

using Merge.Application.Services.Content;

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
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
        services.AddScoped<IReturnRequestService, ReturnRequestService>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<IProductBundleService, ProductBundleService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<ISellerDashboardService, SellerDashboardService>();

        services.AddScoped<IBulkProductService, BulkProductService>();

        services.AddScoped<ISellerOnboardingService, SellerOnboardingService>();
        services.AddScoped<ISharedWishlistService, SharedWishlistService>();
        services.AddScoped<IProductComparisonService, ProductComparisonService>();
        services.AddScoped<ISizeGuideService, SizeGuideService>();
        services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
        services.AddScoped<ISellerFinanceService, SellerFinanceService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IProductTemplateService, ProductTemplateService>();
        services.AddScoped<IPaymentMethodService, PaymentMethodService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IProductQuestionService, ProductQuestionService>();
        services.AddScoped<ISellerCommissionService, SellerCommissionService>();
        services.AddScoped<IFraudDetectionService, FraudDetectionService>();
        services.AddScoped<IPriceOptimizationService, PriceOptimizationService>();
        services.AddScoped<IDemandForecastingService, DemandForecastingService>();
        services.AddScoped<IElasticsearchService, ElasticsearchService>();
        services.AddScoped<IPersonalizationService, PersonalizationService>();
        // #pragma warning restore CS0618

        return services;
    }
}
