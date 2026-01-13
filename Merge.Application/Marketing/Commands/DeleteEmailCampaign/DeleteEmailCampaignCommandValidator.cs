using FluentValidation;

namespace Merge.Application.Marketing.Commands.DeleteEmailCampaign;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class DeleteEmailCampaignCommandValidator() : AbstractValidator<DeleteEmailCampaignCommand>
{
    public DeleteEmailCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kampanya ID'si zorunludur.");
    }
}
