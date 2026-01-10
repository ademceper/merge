using MediatR;

namespace Merge.Application.Marketing.Commands.DeleteEmailCampaign;

public record DeleteEmailCampaignCommand(
    Guid Id) : IRequest<bool>;
