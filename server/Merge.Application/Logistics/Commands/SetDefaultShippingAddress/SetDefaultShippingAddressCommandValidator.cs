using FluentValidation;

namespace Merge.Application.Logistics.Commands.SetDefaultShippingAddress;

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

