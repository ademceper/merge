using FluentValidation;

namespace Merge.Application.Logistics.Commands.DeleteShippingAddress;

public class DeleteShippingAddressCommandValidator : AbstractValidator<DeleteShippingAddressCommand>
{
    public DeleteShippingAddressCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Adres ID'si zorunludur.");
    }
}

