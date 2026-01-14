using FluentValidation;

namespace Merge.Application.Logistics.Commands.CreateShippingAddress;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateShippingAddressCommandValidator : AbstractValidator<CreateShippingAddressCommand>
{
    public CreateShippingAddressCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.Label)
            .MaximumLength(50).WithMessage("Etiket en fazla 50 karakter olabilir.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad zorunludur.")
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.")
            .MinimumLength(2).WithMessage("Ad en az 2 karakter olmalıdır.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad zorunludur.")
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir.")
            .MinimumLength(2).WithMessage("Soyad en az 2 karakter olmalıdır.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefon numarası zorunludur.")
            .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir.");

        RuleFor(x => x.AddressLine1)
            .NotEmpty().WithMessage("Adres satırı zorunludur.")
            .MaximumLength(200).WithMessage("Adres satırı en fazla 200 karakter olabilir.")
            .MinimumLength(5).WithMessage("Adres satırı en az 5 karakter olmalıdır.");

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200).WithMessage("Adres satırı 2 en fazla 200 karakter olabilir.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Şehir zorunludur.")
            .MaximumLength(100).WithMessage("Şehir en fazla 100 karakter olabilir.")
            .MinimumLength(2).WithMessage("Şehir en az 2 karakter olmalıdır.");

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("Eyalet/Bölge en fazla 100 karakter olabilir.");

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Posta kodu en fazla 20 karakter olabilir.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Ülke zorunludur.")
            .MaximumLength(100).WithMessage("Ülke en fazla 100 karakter olabilir.");

        RuleFor(x => x.Instructions)
            .MaximumLength(500).WithMessage("Talimatlar en fazla 500 karakter olabilir.");
    }
}

