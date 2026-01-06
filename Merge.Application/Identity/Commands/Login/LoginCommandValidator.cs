using FluentValidation;

namespace Merge.Application.Identity.Commands.Login;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email adresi zorunludur.")
            .EmailAddress()
            .WithMessage("Geçerli bir email adresi giriniz.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Şifre zorunludur.");
    }
}
