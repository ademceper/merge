using FluentValidation;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Marketing.Commands.SubscribeEmail;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class SubscribeEmailCommandValidator : AbstractValidator<SubscribeEmailCommand>
{
    public SubscribeEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(200).WithMessage("E-posta adresi en fazla 200 karakter olabilir.");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.");

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir.");

        RuleFor(x => x.Source)
            .MaximumLength(100).WithMessage("Kaynak en fazla 100 karakter olabilir.");
    }
}
