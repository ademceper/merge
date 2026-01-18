namespace Merge.Application.DTOs.Logistics;

public record CalculateShippingDto(
    Guid OrderId,
    string Provider
);
