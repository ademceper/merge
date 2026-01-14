using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Create Pre Order Campaign DTO - BOLUM 7.1.5: Records (ZORUNLU)
/// BOLUM 2.1: FluentValidation - Validation CreatePreOrderCampaignCommandValidator'da yapılıyor
/// </summary>
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
