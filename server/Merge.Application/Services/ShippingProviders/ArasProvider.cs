using Merge.Application.DTOs;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.ShippingProviders;
using Merge.Application.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Services.ShippingProviders;

public class ArasProvider(IConfiguration configuration, ILogger<ArasProvider> logger) : IShippingProvider
{
    public string ProviderName => "Aras Kargo";

    public async Task<ShippingProviderResponseDto> CreateShipmentAsync(ShippingProviderRequestDto request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.OrderNumber))
        {
            throw new ValidationException("Sipariş numarası boş olamaz.");
        }

        logger.LogInformation("Aras Kargo shipment creation started for order {OrderNumber}", request.OrderNumber);
        
        var apiKey = configuration["ShippingProviders:Aras:ApiKey"];
        var apiSecret = configuration["ShippingProviders:Aras:ApiSecret"];
        
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            logger.LogWarning("Aras Kargo API credentials not configured");
            return new ShippingProviderResponseDto
            {
                Success = false,
                ErrorMessage = "Shipping provider not configured"
            };
        }

        await Task.Delay(200);
        
        var trackingNumber = $"ARAS{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid():N}".Substring(0, 20).ToUpper();
        var estimatedDelivery = DateTime.UtcNow.AddDays(2);
        
        logger.LogInformation("Aras Kargo shipment created successfully. TrackingNumber: {TrackingNumber}", trackingNumber);
        
        return new ShippingProviderResponseDto
        {
            Success = true,
            TrackingNumber = trackingNumber,
            LabelUrl = $"https://www.araskargo.com.tr/tr/takip?code={trackingNumber}",
            ShippingCost = CalculateCost(request),
            EstimatedDeliveryDate = estimatedDelivery,
            Metadata = new Dictionary<string, object>
            {
                { "provider", "Aras Kargo" },
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

        logger.LogInformation("Aras Kargo tracking check. TrackingNumber: {TrackingNumber}", trackingNumber);
        
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

        logger.LogInformation("Aras Kargo label generation. TrackingNumber: {TrackingNumber}", trackingNumber);
        
        await Task.Delay(100);
        
        return new ShippingLabelDto
        {
            TrackingNumber = trackingNumber,
            LabelUrl = $"https://www.araskargo.com.tr/labels/{trackingNumber}.pdf",
            Format = "PDF"
        };
    }

    public async Task<bool> CancelShipmentAsync(string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new ArgumentNullException(nameof(trackingNumber));
        }

        logger.LogInformation("Aras Kargo shipment cancellation. TrackingNumber: {TrackingNumber}", trackingNumber);
        
        await Task.Delay(100);
        
        return true;
    }

    public async Task<decimal> CalculateShippingCostAsync(ShippingCostRequestDto request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        logger.LogInformation("Aras Kargo cost calculation");
        
        await Task.Delay(50);
        
        var baseCost = 45m;
        var weightCost = request.TotalWeight * 1.8m;
        
        return baseCost + weightCost;
    }

    private decimal CalculateCost(ShippingProviderRequestDto request)
    {
        var baseCost = 45m;
        var weightCost = request.TotalWeight * 1.8m;
        return baseCost + weightCost;
    }
}

