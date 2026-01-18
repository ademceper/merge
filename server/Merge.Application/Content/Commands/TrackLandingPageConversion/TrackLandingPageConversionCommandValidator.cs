using FluentValidation;

namespace Merge.Application.Content.Commands.TrackLandingPageConversion;

public class TrackLandingPageConversionCommandValidator : AbstractValidator<TrackLandingPageConversionCommand>
{
    public TrackLandingPageConversionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

