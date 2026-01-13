using FluentValidation;

namespace Merge.Application.Marketing.Commands.PauseEmailCampaign;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class PauseEmailCampaignCommandValidator() : AbstractValidator<PauseEmailCampaignCommand>
{
    public PauseEmailCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kampanya ID'si zorunludur.");
    }
}
