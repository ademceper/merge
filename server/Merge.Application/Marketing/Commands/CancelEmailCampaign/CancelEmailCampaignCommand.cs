using MediatR;

namespace Merge.Application.Marketing.Commands.CancelEmailCampaign;

public record CancelEmailCampaignCommand(
    Guid Id) : IRequest<bool>;
