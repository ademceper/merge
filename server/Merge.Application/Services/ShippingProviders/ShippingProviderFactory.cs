using Merge.Application.Interfaces.ShippingProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Merge.Application.Services.ShippingProviders;

public class ShippingProviderFactory(IServiceProvider serviceProvider, IConfiguration configuration)
{

    public IShippingProvider GetProvider(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new ArgumentNullException(nameof(providerName));
        }

        return providerName.ToUpper() switch
        {
            "YURTICI" or "YURTICI KARGO" => serviceProvider.GetRequiredService<YurticiProvider>(),
            "ARAS" or "ARAS KARGO" => serviceProvider.GetRequiredService<ArasProvider>(),
            "MNG" or "MNG KARGO" => serviceProvider.GetRequiredService<MNGProvider>(),
            _ => throw new ArgumentException($"Unknown shipping provider: {providerName}")
        };
    }

    public IShippingProvider GetDefaultProvider()
    {
        var defaultProvider = configuration["ShippingProviders:Default"] ?? "Yurtiçi";
        return GetProvider(defaultProvider);
    }
}

