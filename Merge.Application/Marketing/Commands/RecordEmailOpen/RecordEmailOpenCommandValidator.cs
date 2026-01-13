using FluentValidation;

namespace Merge.Application.Marketing.Commands.RecordEmailOpen;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class RecordEmailOpenCommandValidator() : AbstractValidator<RecordEmailOpenCommand>
{
    public RecordEmailOpenCommandValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("Campaign ID zorunludur.");

        RuleFor(x => x.SubscriberId)
            .NotEmpty().WithMessage("Subscriber ID zorunludur.");
    }
}
