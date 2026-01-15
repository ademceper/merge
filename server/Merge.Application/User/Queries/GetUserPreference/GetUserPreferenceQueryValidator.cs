using FluentValidation;

namespace Merge.Application.User.Queries.GetUserPreference;

public class GetUserPreferenceQueryValidator : AbstractValidator<GetUserPreferenceQuery>
{
    public GetUserPreferenceQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
