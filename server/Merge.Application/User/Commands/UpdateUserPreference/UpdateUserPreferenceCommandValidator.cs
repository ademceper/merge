using FluentValidation;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Commands.UpdateUserPreference;

public class UpdateUserPreferenceCommandValidator : AbstractValidator<UpdateUserPreferenceCommand>
{
    public UpdateUserPreferenceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Theme)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Theme))
            .WithMessage("Tema en fazla 50 karakter olabilir.");

        RuleFor(x => x.DefaultLanguage)
            .MaximumLength(10)
            .When(x => !string.IsNullOrEmpty(x.DefaultLanguage))
            .WithMessage("Varsayılan dil en fazla 10 karakter olabilir.");

        RuleFor(x => x.DefaultCurrency)
            .MaximumLength(10)
            .When(x => !string.IsNullOrEmpty(x.DefaultCurrency))
            .WithMessage("Varsayılan para birimi en fazla 10 karakter olabilir.");

        RuleFor(x => x.ItemsPerPage)
            .InclusiveBetween(1, 100)
            .When(x => x.ItemsPerPage.HasValue)
            .WithMessage("Sayfa başına öğe sayısı 1 ile 100 arasında olmalıdır.");

        RuleFor(x => x.DateFormat)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.DateFormat))
            .WithMessage("Tarih formatı en fazla 50 karakter olabilir.");

        RuleFor(x => x.TimeFormat)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.TimeFormat))
            .WithMessage("Saat formatı en fazla 50 karakter olabilir.");

        RuleFor(x => x.DefaultShippingAddress)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.DefaultShippingAddress))
            .WithMessage("Varsayılan kargo adresi en fazla 100 karakter olabilir.");

        RuleFor(x => x.DefaultPaymentMethod)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.DefaultPaymentMethod))
            .WithMessage("Varsayılan ödeme yöntemi en fazla 100 karakter olabilir.");
    }
}
