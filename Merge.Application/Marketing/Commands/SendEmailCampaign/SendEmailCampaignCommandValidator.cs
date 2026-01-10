using FluentValidation;

namespace Merge.Application.Marketing.Commands.SendEmailCampaign;

public class SendEmailCampaignCommandValidator : AbstractValidator<SendEmailCampaignCommand>
{
    public SendEmailCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kampanya ID'si zorunludur.");
    }
}
