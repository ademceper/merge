using MediatR;

namespace Merge.Application.Cart.Commands.DeactivatePreOrderCampaign;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeactivatePreOrderCampaignCommand(
    Guid CampaignId) : IRequest<bool>;

