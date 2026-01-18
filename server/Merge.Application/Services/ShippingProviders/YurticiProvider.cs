using Merge.Application.DTOs;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.ShippingProviders;
using Merge.Application.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Services.ShippingProviders;

public class YurticiProvider(IConfiguration configuration, ILogger<YurticiProvider> logger) : IShippingProvider
{
    public string ProviderName => "Yurtiçi Kargo";

    public async Task<ShippingProviderResponseDto> CreateShipmentAsync(ShippingProviderRequestDto request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.OrderNumber))
        {
            throw new ValidationException("Sipariş numarası boş olamaz.");
        }

        logger.LogInformation("Yurtiçi Kargo shipment creation started for order {OrderNumber}", request.OrderNumber);
        
        var apiKey = configuration["ShippingProviders:Yurtici:ApiKey"];
        var apiSecret = configuration["ShippingProviders:Yurtici:ApiSecret"];
        
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            logger.LogWarning("Yurtiçi Kargo API credentials not configured");
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
        
        logger.LogInformation("Yurtiçi Kargo shipment created successfully. TrackingNumber: {TrackingNumber}", trackingNumber);
        
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
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new ArgumentNullException(nameof(trackingNumber));
        }

        logger.LogInformation("Yurtiçi Kargo tracking check. TrackingNumber: {TrackingNumber}", trackingNumber);
        
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
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new ArgumentNullException(nameof(trackingNumber));
        }

        logger.LogInformation("Yurtiçi Kargo label generation. TrackingNumber: {TrackingNumber}", trackingNumber);
        
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
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new ArgumentNullException(nameof(trackingNumber));
        }

        logger.LogInformation("Yurtiçi Kargo shipment cancellation. TrackingNumber: {TrackingNumber}", trackingNumber);
        
        await Task.Delay(100);
        
        return true;
    }

    public async Task<decimal> CalculateShippingCostAsync(ShippingCostRequestDto request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        logger.LogInformation("Yurtiçi Kargo cost calculation");
        
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

