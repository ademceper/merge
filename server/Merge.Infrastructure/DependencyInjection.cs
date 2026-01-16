using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Merge.Application.Interfaces;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Data.Contexts;
using Merge.Infrastructure.Repositories;
using Merge.Domain.SharedKernel;
using Merge.Infrastructure.Common;
using Merge.Application.Services.PaymentGateways;
using Merge.Application.Services.ShippingProviders;
using Merge.Application.Services.EmailProviders;
using Merge.Application.Services.SmsProviders;

namespace Merge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<OrderingDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<MarketingDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<SupportDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<ContentDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<MarketplaceDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IDbContext, ApplicationDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Payment Gateways
        services.AddScoped<IyzicoGateway>();
        services.AddScoped<PayTRGateway>();
        services.AddScoped<StripeGateway>();
        services.AddScoped<PaymentGatewayFactory>();

        // Shipping Providers
        services.AddScoped<YurticiProvider>();
        services.AddScoped<ArasProvider>();
        services.AddScoped<MNGProvider>();
        services.AddScoped<ShippingProviderFactory>();

        // Email & SMS Providers
        services.AddScoped<SendGridProvider>();
        services.AddScoped<TwilioProvider>();
        services.AddScoped<NetgsmProvider>();

        // ✅ CRITICAL FIX: Background Services - Outbox pattern için event publisher aktif edildi
        services.AddHostedService<Merge.Infrastructure.BackgroundServices.OutboxMessagePublisher>();

        // ✅ CRITICAL FIX: Health Checks aktif edildi
        services.AddHealthChecks()
            .AddNpgSql(connectionString!, name: "postgres", tags: new[] { "db", "postgres", "ready" })
            .AddRedis(configuration.GetConnectionString("Redis") ?? "localhost:6379", name: "redis", tags: new[] { "cache", "redis", "ready" });

        return services;
    }
}
