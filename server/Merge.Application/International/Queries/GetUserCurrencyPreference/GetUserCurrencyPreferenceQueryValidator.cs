using FluentValidation;

namespace Merge.Application.International.Queries.GetUserCurrencyPreference;

public class GetUserCurrencyPreferenceQueryValidator() : AbstractValidator<GetUserCurrencyPreferenceQuery>
{
    public GetUserCurrencyPreferenceQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");
    }
}

