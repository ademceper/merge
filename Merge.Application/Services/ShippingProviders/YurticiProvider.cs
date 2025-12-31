using Merge.Application.DTOs;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.ShippingProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Merge.Application.Services.ShippingProviders;

public class YurticiProvider : IShippingProvider
{
    public string ProviderName => "Yurtiçi Kargo";
    
    private readonly IConfiguration _configuration;
    private readonly ILogger<YurticiProvider> _logger;

    public YurticiProvider(IConfiguration configuration, ILogger<YurticiProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ShippingProviderResponseDto> CreateShipmentAsync(ShippingProviderRequestDto request)
    {
        _logger.LogInformation("Yurtiçi Kargo shipment creation started for order {OrderNumber}", request.OrderNumber);
        
        var apiKey = _configuration["ShippingProviders:Yurtici:ApiKey"];
        var apiSecret = _configuration["ShippingProviders:Yurtici:ApiSecret"];
        
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            _logger.LogWarning("Yurtiçi Kargo API credentials not configured");
            return new ShippingProviderResponseDto
            {
                Success = false,
                ErrorMessage = "Shipping provider not configured"
            };
        }

        // Mock implementation - Gerçek implementasyonda Yurtiçi Kargo API kullanılacak
        await Task.Delay(200);
        
        var trackingNumber = $"YURTICI{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid():N}".Substring(0, 20).ToUpper();
        var estimatedDelivery = DateTime.UtcNow.AddDays(3);
        
        _logger.LogInformation("Yurtiçi Kargo shipment created successfully. TrackingNumber: {TrackingNumber}", trackingNumber);
        
        return new ShippingProviderResponseDto
        {
            Success = true,
            TrackingNumber = trackingNumber,
            LabelUrl = $"https://www.yurticikargo.com/tr/online-servisler/gonderi-sorgula?code={trackingNumber}",
            ShippingCost = CalculateCost(request),
            EstimatedDeliveryDate = estimatedDelivery,
            Metadata = new Dictionary<string, object>
            {
                { "provider", "Yurtiçi Kargo" },
                { "orderNumber", request.OrderNumber }
            }
        };
    }

    public async Task<ShippingTrackingDto> GetTrackingAsync(string trackingNumber)
    {
        _logger.LogInformation("Yurtiçi Kargo tracking check. TrackingNumber: {TrackingNumber}", trackingNumber);
        
        await Task.Delay(100);
        
        return new ShippingTrackingDto
        {
            TrackingNumber = trackingNumber,
            Status = "In Transit",
            Events = new List<ShippingTrackingEventDto>
            {
                new()
                {
                    Date = DateTime.UtcNow.AddDays(-1),
                    Status = "Picked Up",
                    Location = "İstanbul",
                    Description = "Kargo alındı"
                },
                new()
                {
                    Date = DateTime.UtcNow,
                    Status = "In Transit",
                    Location = "Ankara",
                    Description = "Dağıtım merkezine ulaştı"
                }
            },
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(1)
        };
    }

    public async Task<ShippingLabelDto> GetShippingLabelAsync(string trackingNumber)
    {
        _logger.LogInformation("Yurtiçi Kargo label generation. TrackingNumber: {TrackingNumber}", trackingNumber);
        
        await Task.Delay(100);
        
        return new ShippingLabelDto
        {
            TrackingNumber = trackingNumber,
            LabelUrl = $"https://www.yurticikargo.com/labels/{trackingNumber}.pdf",
            Format = "PDF"
        };
    }

    public async Task<bool> CancelShipmentAsync(string trackingNumber)
    {
        _logger.LogInformation("Yurtiçi Kargo shipment cancellation. TrackingNumber: {TrackingNumber}", trackingNumber);
        
        await Task.Delay(100);
        
        return true;
    }

    public async Task<decimal> CalculateShippingCostAsync(ShippingCostRequestDto request)
    {
        _logger.LogInformation("Yurtiçi Kargo cost calculation");
        
        await Task.Delay(50);
        
        // Base cost + weight based
        var baseCost = 50m;
        var weightCost = request.TotalWeight * 2m; // 2 TL per kg
        
        return baseCost + weightCost;
    }

    private decimal CalculateCost(ShippingProviderRequestDto request)
    {
        var baseCost = 50m;
        var weightCost = request.TotalWeight * 2m;
        return baseCost + weightCost;
    }
}

