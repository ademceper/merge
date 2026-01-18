using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.CreatePreOrderCampaign;

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

