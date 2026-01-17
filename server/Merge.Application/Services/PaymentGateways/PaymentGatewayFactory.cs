using Merge.Application.Interfaces.PaymentGateways;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Merge.Application.Services.PaymentGateways;

public class PaymentGatewayFactory(IServiceProvider serviceProvider, IConfiguration configuration)
{

    public IPaymentGateway GetGateway(string gatewayName)
    {
        return gatewayName.ToUpper() switch
        {
            "IYZICO" => serviceProvider.GetRequiredService<IyzicoGateway>(),
            "PAYTR" => serviceProvider.GetRequiredService<PayTRGateway>(),
            "STRIPE" => serviceProvider.GetRequiredService<StripeGateway>(),
            _ => throw new ArgumentException($"Unknown payment gateway: {gatewayName}")
        };
    }

    public IPaymentGateway GetDefaultGateway()
    {
        var defaultGateway = configuration["PaymentGateways:Default"] ?? "Iyzico";
        return GetGateway(defaultGateway);
    }
}

