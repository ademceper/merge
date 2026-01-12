using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Commands.ResetUserPreference;

public class ResetUserPreferenceCommandValidator : AbstractValidator<ResetUserPreferenceCommand>
{
    public ResetUserPreferenceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");
    }
}
