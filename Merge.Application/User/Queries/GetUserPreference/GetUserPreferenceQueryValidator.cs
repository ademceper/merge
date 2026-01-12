using FluentValidation;

namespace Merge.Application.User.Queries.GetUserPreference;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetUserPreferenceQueryValidator : AbstractValidator<GetUserPreferenceQuery>
{
    public GetUserPreferenceQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
