using FluentValidation;

namespace Merge.Application.Marketing.Commands.CancelEmailCampaign;

public class CancelEmailCampaignCommandValidator : AbstractValidator<CancelEmailCampaignCommand>
{
    public CancelEmailCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kampanya ID'si zorunludur.");
    }
}
