using FluentValidation;
using Microsoft.Extensions.Options;
using Merge.Application.Configuration;

namespace Merge.Application.International.Queries.GetCurrencyByCode;

public class GetCurrencyByCodeQueryValidator(IOptions<InternationalSettings> settings) : AbstractValidator<GetCurrencyByCodeQuery>
{
    private readonly InternationalSettings config = settings.Value;

    public GetCurrencyByCodeQueryValidator() : this(Options.Create(new InternationalSettings()))
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Para birimi kodu zorunludur.")
            .Length(3, config.MaxCurrencyCodeLength)
            .WithMessage($"Para birimi kodu en az 3, en fazla {config.MaxCurrencyCodeLength} karakter olmalıdır.")
            .Matches("^[A-Z]{3}$").WithMessage("Para birimi kodu 3 büyük harften oluşmalıdır (örn: USD).");
    }
}

