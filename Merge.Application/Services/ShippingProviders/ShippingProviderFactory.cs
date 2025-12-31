using Merge.Application.Interfaces.ShippingProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Merge.Application.Services.ShippingProviders;

public class ShippingProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public ShippingProviderFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public IShippingProvider GetProvider(string providerName)
    {
        // ✅ ARCHITECTURE: Null check (ZORUNLU)
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new ArgumentNullException(nameof(providerName));
        }

        return providerName.ToUpper() switch
        {
            "YURTICI" or "YURTICI KARGO" => _serviceProvider.GetRequiredService<YurticiProvider>(),
            "ARAS" or "ARAS KARGO" => _serviceProvider.GetRequiredService<ArasProvider>(),
            "MNG" or "MNG KARGO" => _serviceProvider.GetRequiredService<MNGProvider>(),
            _ => throw new ArgumentException($"Unknown shipping provider: {providerName}")
        };
    }

    public IShippingProvider GetDefaultProvider()
    {
        var defaultProvider = _configuration["ShippingProviders:Default"] ?? "Yurtiçi";
        return GetProvider(defaultProvider);
    }
}

