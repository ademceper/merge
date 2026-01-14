using FluentValidation;

namespace Merge.Application.Logistics.Commands.SetDefaultShippingAddress;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class SetDefaultShippingAddressCommandValidator : AbstractValidator<SetDefaultShippingAddressCommand>
{
    public SetDefaultShippingAddressCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si zorunludur.");

        RuleFor(x => x.AddressId)
            .NotEmpty().WithMessage("Adres ID'si zorunludur.");
    }
}

