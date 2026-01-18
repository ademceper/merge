namespace Merge.Application.DTOs.Logistics;

public record ShippingProviderDto(
    string Code,
    string Name,
    decimal BaseCost,
    int EstimatedDays
);
