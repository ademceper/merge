using MediatR;

namespace Merge.Application.Marketing.Commands.SendEmailCampaign;

public record SendEmailCampaignCommand(
    Guid Id) : IRequest<bool>;
