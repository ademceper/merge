using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Queries.GetLanguageByCode;

public class GetLanguageByCodeQueryValidator : AbstractValidator<GetLanguageByCodeQuery>
{
    private readonly InternationalSettings config;

    public GetLanguageByCodeQueryValidator(IOptions<InternationalSettings> settings)
    {
        config = settings.Value;

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(config.MinLanguageCodeLength, config.MaxLanguageCodeLength)
            .WithMessage($"Dil kodu en az {config.MinLanguageCodeLength}, en fazla {config.MaxLanguageCodeLength} karakter olmalıdır.");
    }
}

