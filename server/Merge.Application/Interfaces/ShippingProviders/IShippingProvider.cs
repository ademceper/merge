using Merge.Application.DTOs;

namespace Merge.Application.Interfaces.ShippingProviders;

public interface IShippingProvider
{
    string ProviderName { get; }
    Task<ShippingProviderResponseDto> CreateShipmentAsync(ShippingProviderRequestDto request);
    Task<ShippingTrackingDto> GetTrackingAsync(string trackingNumber);
    Task<ShippingLabelDto> GetShippingLabelAsync(string trackingNumber);
    Task<bool> CancelShipmentAsync(string trackingNumber);
    Task<decimal> CalculateShippingCostAsync(ShippingCostRequestDto request);
}

public class ShippingProviderRequestDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public ShippingProviderAddressDto SenderAddress { get; set; } = new();
    public ShippingProviderAddressDto ReceiverAddress { get; set; } = new();
    public List<ShippingItemDto> Items { get; set; } = new();
    public decimal TotalWeight { get; set; }
    public decimal TotalValue { get; set; }
    public string? ServiceType { get; set; } // Express, Standard, etc.
    public Dictionary<string, string>? Metadata { get; set; }
}

public class ShippingProviderAddressDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "TR";
}

public class ShippingItemDto
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Weight { get; set; }
    public decimal Value { get; set; }
}

public class ShippingProviderResponseDto
{
    public bool Success { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public string? LabelUrl { get; set; }
    public decimal ShippingCost { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class ShippingTrackingDto
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<ShippingTrackingEventDto> Events { get; set; } = new();
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? DeliveredDate { get; set; }
}

public class ShippingTrackingEventDto
{
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class ShippingLabelDto
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string LabelUrl { get; set; } = string.Empty;
    public byte[]? LabelData { get; set; }
    public string Format { get; set; } = "PDF";
}

public class ShippingCostRequestDto
{
    public ShippingProviderAddressDto FromAddress { get; set; } = new();
    public ShippingProviderAddressDto ToAddress { get; set; } = new();
    public decimal TotalWeight { get; set; }
    public decimal TotalValue { get; set; }
    public string? ServiceType { get; set; }
}
