using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Commands.PatchPreOrderCampaign;

/// <summary>
/// PATCH command for partial pre-order campaign updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchPreOrderCampaignCommand(
    Guid CampaignId,
    PatchPreOrderCampaignDto PatchDto
) : IRequest<bool>;
