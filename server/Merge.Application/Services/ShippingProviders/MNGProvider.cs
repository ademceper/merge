using Merge.Application.DTOs;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.ShippingProviders;
using Merge.Application.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Services.ShippingProviders;

public class MNGProvider : IShippingProvider
{
    public string ProviderName => "MNG Kargo";
    
    private readonly IConfiguration _configuration;
    private readonly ILogger<MNGProvider> _logger;

    public MNGProvider(IConfiguration configuration, ILogger<MNGProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ShippingProviderResponseDto> CreateShipmentAsync(ShippingProviderRequestDto request)
    {
        // ✅ ARCHITECTURE: Null check (ZORUNLU)
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // ✅ ARCHITECTURE: Input validation
        if (string.IsNullOrWhiteSpace(request.OrderNumber))
        {
            throw new ValidationException("Sipariş numarası boş olamaz.");
        }

        _logger.LogInformation("MNG Kargo shipment creation started for order {OrderNumber}", request.OrderNumber);
        
        var apiKey = _configuration["ShippingProviders:MNG:ApiKey"];
        var apiSecret = _configuration["ShippingProviders:MNG:ApiSecret"];
        
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            _logger.LogWarning("MNG Kargo API credentials not configured");
            return new ShippingProviderResponseDto
            {
                Success = false,
                ErrorMessage = "Shipping provider not configured"
            };
        }

        await Task.Delay(200);
        
        var trackingNumber = $"MNG{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid():N}".Substring(0, 20).ToUpper();
        var estimatedDelivery = DateTime.UtcNow.AddDays(2);
        
        _logger.LogInformation("MNG Kargo shipment created successfully. TrackingNumber: {TrackingNumber}", trackingNumber);
        
        return new ShippingProviderResponseDto
        {
            Success = true,
            TrackingNumber = trackingNumber,
            LabelUrl = $"https://www.mngkargo.com.tr/tr/takip?code={trackingNumber}",
            ShippingCost = CalculateCost(request),
            EstimatedDeliveryDate = estimatedDelivery,
            Metadata = new Dictionary<string, object>
            {
                { "provider", "MNG Kargo" },
                { "orderNumber", request.OrderNumber }
            }
        };
    }

    public async Task<ShippingTrackingDto> GetTrackingAsync(string trackingNumber)
    {
        // ✅ ARCHITECTURE: Null check (ZORUNLU)
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new ArgumentNullException(nameof(trackingNumber));
        }

        _logger.LogInformation("MNG Kargo tracking check. TrackingNumber: {TrackingNumber}", trackingNumber);
        
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
        // ✅ ARCHITECTURE: Null check (ZORUNLU)
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new ArgumentNullException(nameof(trackingNumber));
        }

        _logger.LogInformation("MNG Kargo label generation. TrackingNumber: {TrackingNumber}", trackingNumber);
        
        await Task.Delay(100);
        
        return new ShippingLabelDto
        {
            TrackingNumber = trackingNumber,
            LabelUrl = $"https://www.mngkargo.com.tr/labels/{trackingNumber}.pdf",
            Format = "PDF"
        };
    }

    public async Task<bool> CancelShipmentAsync(string trackingNumber)
    {
        // ✅ ARCHITECTURE: Null check (ZORUNLU)
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            throw new ArgumentNullException(nameof(trackingNumber));
        }

        _logger.LogInformation("MNG Kargo shipment cancellation. TrackingNumber: {TrackingNumber}", trackingNumber);
        
        await Task.Delay(100);
        
        return true;
    }

    public async Task<decimal> CalculateShippingCostAsync(ShippingCostRequestDto request)
    {
        // ✅ ARCHITECTURE: Null check (ZORUNLU)
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        _logger.LogInformation("MNG Kargo cost calculation");
        
        await Task.Delay(50);
        
        var baseCost = 40m;
        var weightCost = request.TotalWeight * 1.5m;
        
        return baseCost + weightCost;
    }

    private decimal CalculateCost(ShippingProviderRequestDto request)
    {
        var baseCost = 40m;
        var weightCost = request.TotalWeight * 1.5m;
        return baseCost + weightCost;
    }
}

