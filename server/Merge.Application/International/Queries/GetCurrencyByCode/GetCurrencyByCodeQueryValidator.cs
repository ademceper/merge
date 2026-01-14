using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Queries.GetCurrencyByCode;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 12.0: Configuration - Magic number'lar configuration'dan alınıyor
public class GetCurrencyByCodeQueryValidator : AbstractValidator<GetCurrencyByCodeQuery>
{
    public GetCurrencyByCodeQueryValidator(IOptions<InternationalSettings> settings)
    {
        var config = settings.Value;

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, config.MaxCurrencyCodeLength)
            .WithMessage($"Para birimi kodu en az 3, en fazla {config.MaxCurrencyCodeLength} karakter olmalıdır.")
            .Matches("^[A-Z]{3}$").WithMessage("Para birimi kodu 3 büyük harften oluşmalıdır (örn: USD).");
    }
}

