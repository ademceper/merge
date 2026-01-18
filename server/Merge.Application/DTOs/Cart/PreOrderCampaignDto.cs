using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;


public record PreOrderCampaignDto(
    Guid Id,
    string Name,
    string Description,
    Guid ProductId,
    string ProductName,
    string ProductImage,
    DateTime StartDate,
    DateTime EndDate,
    DateTime ExpectedDeliveryDate,
    int MaxQuantity,
    int CurrentQuantity,
    int AvailableQuantity,
    decimal DepositPercentage,
    decimal SpecialPrice,
    bool IsActive,
    bool IsFull
);
