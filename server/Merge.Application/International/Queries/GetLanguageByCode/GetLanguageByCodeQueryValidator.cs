using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Queries.GetLanguageByCode;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class GetLanguageByCodeQueryValidator : AbstractValidator<GetLanguageByCodeQuery>
{
    public GetLanguageByCodeQueryValidator(IOptions<InternationalSettings> settings)
    {
        var config = settings.Value;

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Dil kodu zorunludur.")
            .Length(config.MinLanguageCodeLength, config.MaxLanguageCodeLength)
            .WithMessage($"Dil kodu en az {config.MinLanguageCodeLength}, en fazla {config.MaxLanguageCodeLength} karakter olmalıdır.");
    }
}

