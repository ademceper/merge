using Merge.Application.Interfaces.PaymentGateways;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Merge.Application.Services.PaymentGateways;

public class PaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public PaymentGatewayFactory(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public IPaymentGateway GetGateway(string gatewayName)
    {
        return gatewayName.ToUpper() switch
        {
            "IYZICO" => _serviceProvider.GetRequiredService<IyzicoGateway>(),
            "PAYTR" => _serviceProvider.GetRequiredService<PayTRGateway>(),
            "STRIPE" => _serviceProvider.GetRequiredService<StripeGateway>(),
            _ => throw new ArgumentException($"Unknown payment gateway: {gatewayName}")
        };
    }

    public IPaymentGateway GetDefaultGateway()
    {
        var defaultGateway = _configuration["PaymentGateways:Default"] ?? "Iyzico";
        return GetGateway(defaultGateway);
    }
}

