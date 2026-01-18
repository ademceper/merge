using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

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

