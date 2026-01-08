using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Commands.CreatePreOrderCampaign;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreatePreOrderCampaignCommand(
    string Name,
    string Description,
    Guid ProductId,
    DateTime StartDate,
    DateTime EndDate,
    DateTime ExpectedDeliveryDate,
    int MaxQuantity,
    decimal DepositPercentage,
    decimal SpecialPrice,
    bool NotifyOnAvailable) : IRequest<PreOrderCampaignDto>;

