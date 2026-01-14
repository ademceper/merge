namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record ShippingProviderDto(
    string Code,
    string Name,
    decimal BaseCost,
    int EstimatedDays
);
