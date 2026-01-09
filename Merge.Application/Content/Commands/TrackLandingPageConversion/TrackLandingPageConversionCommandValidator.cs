using FluentValidation;

namespace Merge.Application.Content.Commands.TrackLandingPageConversion;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class TrackLandingPageConversionCommandValidator : AbstractValidator<TrackLandingPageConversionCommand>
{
    public TrackLandingPageConversionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

