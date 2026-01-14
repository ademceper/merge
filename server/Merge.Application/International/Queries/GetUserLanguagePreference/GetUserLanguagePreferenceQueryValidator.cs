using FluentValidation;

namespace Merge.Application.International.Queries.GetUserLanguagePreference;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetUserLanguagePreferenceQueryValidator : AbstractValidator<GetUserLanguagePreferenceQuery>
{
    public GetUserLanguagePreferenceQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur.");
    }
}

