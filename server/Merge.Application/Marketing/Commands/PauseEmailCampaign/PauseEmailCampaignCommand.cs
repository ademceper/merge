using MediatR;

namespace Merge.Application.Marketing.Commands.PauseEmailCampaign;

public record PauseEmailCampaignCommand(
    Guid Id) : IRequest<bool>;
