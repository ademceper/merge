using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Queries.GetProductTranslation;

public class GetProductTranslationQueryValidator : AbstractValidator<GetProductTranslationQuery>
{
    private readonly InternationalSettings config;

    public GetProductTranslationQueryValidator(IOptions<InternationalSettings> settings)
    {
        config = settings.Value;

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Ürün ID'si zorunludur.");

        RuleFor(x => x.LanguageCode)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(config.MinLanguageCodeLength, config.MaxLanguageCodeLength)
            .WithMessage($"Dil kodu en az {config.MinLanguageCodeLength}, en fazla {config.MaxLanguageCodeLength} karakter olmalıdır.");
    }
}

