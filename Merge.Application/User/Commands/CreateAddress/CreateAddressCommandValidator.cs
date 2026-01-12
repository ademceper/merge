using FluentValidation;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Application.User.Commands.CreateAddress;

public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("Kullanici ID'si zorunludur.");

        RuleFor(x => x.Title)
            .MaximumLength(50)
            .WithMessage("Baslik en fazla 50 karakter olabilir.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Ad zorunludur.")
            .MinimumLength(2)
            .WithMessage("Ad en az 2 karakter olmalidir.")
            .MaximumLength(100)
            .WithMessage("Ad en fazla 100 karakter olabilir.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Soyad zorunludur.")
            .MinimumLength(2)
            .WithMessage("Soyad en az 2 karakter olmalidir.")
            .MaximumLength(100)
            .WithMessage("Soyad en fazla 100 karakter olabilir.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Telefon numarasi zorunludur.")
            .MaximumLength(20)
            .WithMessage("Telefon numarasi en fazla 20 karakter olabilir.");

        RuleFor(x => x.AddressLine1)
            .NotEmpty()
            .WithMessage("Adres satiri zorunludur.")
            .MinimumLength(5)
            .WithMessage("Adres satiri en az 5 karakter olmalidir.")
            .MaximumLength(200)
            .WithMessage("Adres satiri en fazla 200 karakter olabilir.");

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.AddressLine2))
            .WithMessage("Adres satiri 2 en fazla 200 karakter olabilir.");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("Sehir zorunludur.")
            .MinimumLength(2)
            .WithMessage("Sehir en az 2 karakter olmalidir.")
            .MaximumLength(100)
            .WithMessage("Sehir en fazla 100 karakter olabilir.");

        RuleFor(x => x.District)
            .MaximumLength(100)
            .WithMessage("Ilce en fazla 100 karakter olabilir.");

        RuleFor(x => x.PostalCode)
            .MaximumLength(10)
            .WithMessage("Posta kodu en fazla 10 karakter olabilir.");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Ulke zorunludur.")
            .MaximumLength(100)
            .WithMessage("Ulke en fazla 100 karakter olabilir.");
    }
}
