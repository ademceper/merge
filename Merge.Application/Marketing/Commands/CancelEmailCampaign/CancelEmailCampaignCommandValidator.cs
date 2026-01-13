using FluentValidation;

namespace Merge.Application.Marketing.Commands.CancelEmailCampaign;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class CancelEmailCampaignCommandValidator : AbstractValidator<CancelEmailCampaignCommand>
{
    public CancelEmailCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kampanya ID'si zorunludur.");
    }
}
