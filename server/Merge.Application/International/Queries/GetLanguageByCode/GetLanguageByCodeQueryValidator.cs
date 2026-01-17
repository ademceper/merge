using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Queries.GetLanguageByCode;

public class GetLanguageByCodeQueryValidator(IOptions<InternationalSettings> settings) : AbstractValidator<GetLanguageByCodeQuery>
{
    private readonly InternationalSettings config = settings.Value;

    public GetLanguageByCodeQueryValidator() : this(Options.Create(new InternationalSettings()))
    {

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(config.MinLanguageCodeLength, config.MaxLanguageCodeLength)
            .WithMessage($"Dil kodu en az {config.MinLanguageCodeLength}, en fazla {config.MaxLanguageCodeLength} karakter olmalıdır.");
    }
}

