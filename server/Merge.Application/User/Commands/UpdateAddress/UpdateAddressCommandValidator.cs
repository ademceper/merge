using FluentValidation;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Application.User.Commands.UpdateAddress;

public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Adres ID'si zorunludur.");

        RuleFor(x => x.Title)
            .MaximumLength(50)
            .WithMessage("Başlık en fazla 50 karakter olabilir.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Ad zorunludur.")
            .MinimumLength(2)
            .WithMessage("Ad en az 2 karakter olmalıdır.")
            .MaximumLength(100)
            .WithMessage("Ad en fazla 100 karakter olabilir.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Soyad zorunludur.")
            .MinimumLength(2)
            .WithMessage("Soyad en az 2 karakter olmalıdır.")
            .MaximumLength(100)
            .WithMessage("Soyad en fazla 100 karakter olabilir.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Telefon numarası zorunludur.")
            .MaximumLength(20)
            .WithMessage("Telefon numarası en fazla 20 karakter olabilir.");

        RuleFor(x => x.AddressLine1)
            .NotEmpty()
            .WithMessage("Adres satırı zorunludur.")
            .MinimumLength(5)
            .WithMessage("Adres satırı en az 5 karakter olmalıdır.")
            .MaximumLength(200)
            .WithMessage("Adres satırı en fazla 200 karakter olabilir.");

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.AddressLine2))
            .WithMessage("Adres satırı 2 en fazla 200 karakter olabilir.");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("Şehir zorunludur.")
            .MinimumLength(2)
            .WithMessage("Şehir en az 2 karakter olmalıdır.")
            .MaximumLength(100)
            .WithMessage("Şehir en fazla 100 karakter olabilir.");

        RuleFor(x => x.District)
            .MaximumLength(100)
            .WithMessage("İlçe en fazla 100 karakter olabilir.");

        RuleFor(x => x.PostalCode)
            .MaximumLength(10)
            .WithMessage("Posta kodu en fazla 10 karakter olabilir.");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Ülke zorunludur.")
            .MaximumLength(100)
            .WithMessage("Ülke en fazla 100 karakter olabilir.");
    }
}
