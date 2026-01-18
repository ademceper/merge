using FluentValidation;

namespace Merge.Application.Logistics.Commands.UpdateShippingAddress;

public class UpdateShippingAddressCommandValidator : AbstractValidator<UpdateShippingAddressCommand>
{
    public UpdateShippingAddressCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Adres ID'si zorunludur.");

        RuleFor(x => x.Label)
            .MaximumLength(50).WithMessage("Etiket en fazla 50 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Label));

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("Ad en fazla 100 karakter olabilir.")
            .MinimumLength(2).WithMessage("Ad en az 2 karakter olmalıdır.")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Soyad en fazla 100 karakter olabilir.")
            .MinimumLength(2).WithMessage("Soyad en az 2 karakter olmalıdır.")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.AddressLine1)
            .MaximumLength(200).WithMessage("Adres satırı en fazla 200 karakter olabilir.")
            .MinimumLength(5).WithMessage("Adres satırı en az 5 karakter olmalıdır.")
            .When(x => !string.IsNullOrEmpty(x.AddressLine1));

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200).WithMessage("Adres satırı 2 en fazla 200 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.AddressLine2));

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("Şehir en fazla 100 karakter olabilir.")
            .MinimumLength(2).WithMessage("Şehir en az 2 karakter olmalıdır.")
            .When(x => !string.IsNullOrEmpty(x.City));

        RuleFor(x => x.State)
            .MaximumLength(100).WithMessage("Eyalet/Bölge en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.State));

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Posta kodu en fazla 20 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.PostalCode));

        RuleFor(x => x.Country)
            .MaximumLength(100).WithMessage("Ülke en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.Instructions)
            .MaximumLength(500).WithMessage("Talimatlar en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Instructions));
    }
}

