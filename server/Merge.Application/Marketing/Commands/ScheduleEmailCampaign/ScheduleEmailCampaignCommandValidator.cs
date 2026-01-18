using FluentValidation;

namespace Merge.Application.Marketing.Commands.ScheduleEmailCampaign;

public class ScheduleEmailCampaignCommandValidator : AbstractValidator<ScheduleEmailCampaignCommand>
{
    public ScheduleEmailCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kampanya ID'si zorunludur.");

        RuleFor(x => x.ScheduledAt)
            .NotEmpty().WithMessage("Planlanan tarih zorunludur.")
            .Must(scheduledAt => scheduledAt > DateTime.UtcNow)
            .WithMessage("Planlanan tarih gelecekte olmalıdır.");
    }
}
