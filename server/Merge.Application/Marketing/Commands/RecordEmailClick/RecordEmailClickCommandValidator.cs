using FluentValidation;

namespace Merge.Application.Marketing.Commands.RecordEmailClick;

public class RecordEmailClickCommandValidator : AbstractValidator<RecordEmailClickCommand>
{
    public RecordEmailClickCommandValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("Campaign ID zorunludur.");

        RuleFor(x => x.SubscriberId)
            .NotEmpty().WithMessage("Subscriber ID zorunludur.");
    }
}
