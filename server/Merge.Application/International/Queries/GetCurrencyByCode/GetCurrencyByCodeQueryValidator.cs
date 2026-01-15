using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Queries.GetCurrencyByCode;

public class GetCurrencyByCodeQueryValidator : AbstractValidator<GetCurrencyByCodeQuery>
{
    private readonly InternationalSettings config;

    public GetCurrencyByCodeQueryValidator(IOptions<InternationalSettings> settings)
    {
        config = settings.Value;

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, config.MaxCurrencyCodeLength)
            .WithMessage($"Para birimi kodu en az 3, en fazla {config.MaxCurrencyCodeLength} karakter olmalıdır.")
            .Matches("^[A-Z]{3}$").WithMessage("Para birimi kodu 3 büyük harften oluşmalıdır (örn: USD).");
    }
}

