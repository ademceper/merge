using FluentValidation;

namespace Merge.Application.Marketing.Commands.DeleteEmailCampaign;

public class DeleteEmailCampaignCommandValidator : AbstractValidator<DeleteEmailCampaignCommand>
{
    public DeleteEmailCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kampanya ID'si zorunludur.");
    }
}
