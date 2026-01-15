using FluentValidation;

namespace Merge.Application.International.Queries.GetUserLanguagePreference;

public class GetUserLanguagePreferenceQueryValidator : AbstractValidator<GetUserLanguagePreferenceQuery>
{
    public GetUserLanguagePreferenceQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");
    }
}

