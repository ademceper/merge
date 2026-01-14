using FluentValidation;

namespace Merge.Application.International.Queries.GetUserCurrencyPreference;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetUserCurrencyPreferenceQueryValidator : AbstractValidator<GetUserCurrencyPreferenceQuery>
{
    public GetUserCurrencyPreferenceQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");
    }
}

