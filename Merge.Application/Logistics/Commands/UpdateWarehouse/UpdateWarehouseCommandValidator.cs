using FluentValidation;

namespace Merge.Application.Logistics.Commands.UpdateWarehouse;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Depo ID'si zorunludur.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Depo adı zorunludur.")
            .MaximumLength(200).WithMessage("Depo adı en fazla 200 karakter olabilir.")
            .MinimumLength(2).WithMessage("Depo adı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres zorunludur.")
            .MaximumLength(500).WithMessage("Adres en fazla 500 karakter olabilir.")
            .MinimumLength(5).WithMessage("Adres en az 5 karakter olmalıdır.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Şehir zorunludur.")
            .MaximumLength(100).WithMessage("Şehir en fazla 100 karakter olabilir.")
            .MinimumLength(2).WithMessage("Şehir en az 2 karakter olmalıdır.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Ülke zorunludur.")
            .MaximumLength(100).WithMessage("Ülke en fazla 100 karakter olabilir.");

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithMessage("Posta kodu en fazla 20 karakter olabilir.");

        RuleFor(x => x.ContactPerson)
            .NotEmpty().WithMessage("İletişim kişisi zorunludur.")
            .MaximumLength(200).WithMessage("İletişim kişisi en fazla 200 karakter olabilir.")
            .MinimumLength(2).WithMessage("İletişim kişisi en az 2 karakter olmalıdır.");

        RuleFor(x => x.ContactPhone)
            .NotEmpty().WithMessage("İletişim telefonu zorunludur.")
            .MaximumLength(20).WithMessage("İletişim telefonu en fazla 20 karakter olabilir.");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("İletişim e-postası zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(200).WithMessage("İletişim e-postası en fazla 200 karakter olabilir.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Kapasite 0'dan büyük olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");
    }
}

