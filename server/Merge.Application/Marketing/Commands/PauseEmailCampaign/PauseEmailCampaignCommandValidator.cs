using FluentValidation;

namespace Merge.Application.Marketing.Commands.PauseEmailCampaign;

public class PauseEmailCampaignCommandValidator : AbstractValidator<PauseEmailCampaignCommand>
{
    public PauseEmailCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kampanya ID'si zorunludur.");
    }
}
