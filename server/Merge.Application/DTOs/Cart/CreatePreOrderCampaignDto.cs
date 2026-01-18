using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;


public record CreatePreOrderCampaignDto(
    string Name,
    string Description,
    Guid ProductId,
    DateTime StartDate,
    DateTime EndDate,
    DateTime ExpectedDeliveryDate,
    int MaxQuantity,
    decimal DepositPercentage,
    decimal SpecialPrice,
    bool NotifyOnAvailable
);
