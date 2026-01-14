using FluentValidation;

namespace Merge.Application.Marketing.Commands.SendEmailCampaign;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class SendEmailCampaignCommandValidator : AbstractValidator<SendEmailCampaignCommand>
{
    public SendEmailCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kampanya ID'si zorunludur.");
    }
}
