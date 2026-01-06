using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Commands.UpdatePreOrderCampaign;

public record UpdatePreOrderCampaignCommand(
    Guid CampaignId,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    DateTime ExpectedDeliveryDate,
    int MaxQuantity,
    decimal DepositPercentage,
    decimal SpecialPrice) : IRequest<bool>;

