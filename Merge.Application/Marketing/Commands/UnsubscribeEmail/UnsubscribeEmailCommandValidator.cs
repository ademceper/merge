using FluentValidation;

namespace Merge.Application.Marketing.Commands.UnsubscribeEmail;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class UnsubscribeEmailCommandValidator : AbstractValidator<UnsubscribeEmailCommand>
{
    public UnsubscribeEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(200).WithMessage("E-posta adresi en fazla 200 karakter olabilir.");
    }
}
